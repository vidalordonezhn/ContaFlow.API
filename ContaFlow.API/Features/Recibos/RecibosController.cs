using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Recibos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ContaFlow.API.Features.Recibos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecibosController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public RecibosController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReciboResponseDto>>> GetRecibos()
        {
            var recibos = await _context.Recibos
                .Include(r => r.PagoHonorario)
                .Include(r => r.Cliente)
                .OrderByDescending(r => r.FechaEmision)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            var dtos = recibos.Select(MapToResponseDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReciboResponseDto>> GetReciboById(int id)
        {
            var recibo = await _context.Recibos
                .Include(r => r.PagoHonorario)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recibo == null)
            {
                return NotFound(new { mensaje = "Recibo no encontrado." });
            }

            return Ok(MapToResponseDto(recibo));
        }

        [HttpPost]
        public async Task<ActionResult<ReciboResponseDto>> CrearRecibo([FromBody] ReciboCreateDto dto)
        {
            Cliente? cliente = null;
            if (dto.ClienteId.HasValue && dto.ClienteId.Value > 0)
            {
                cliente = await _context.Clientes.FindAsync(dto.ClienteId.Value);
            }

            var nombreCliente = cliente?.NombreRazonSocial ?? dto.NombreCliente?.Trim() ?? "Cliente General";
            var rtnCliente = cliente?.Rtn ?? dto.RtnCliente?.Trim() ?? "N/A";

            var esConCai = string.Equals(dto.TipoComprobante, "ConCAI", StringComparison.OrdinalIgnoreCase);

            // Generar o asignar número de recibo / fiscal
            var anioActual = DateTime.UtcNow.Year;
            string numeroRecibo = dto.NumeroRecibo?.Trim() ?? string.Empty;
            string? numeroFiscal = null;
            string? caiCodigo = null;
            string? rangoAutorizado = null;
            DateTime? fechaLimite = null;
            int? caiId = null;

            if (esConCai)
            {
                var caiActivo = await _context.AutorizacionesCAI
                    .Where(c => c.Activo && c.CorrelativoActual <= c.RangoFinal && c.FechaLimiteEmision >= DateTime.UtcNow)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (caiActivo != null)
                {
                    numeroFiscal = caiActivo.SiguienteNumeroFormateado;
                    numeroRecibo = string.IsNullOrWhiteSpace(numeroRecibo) ? numeroFiscal : numeroRecibo;
                    caiCodigo = caiActivo.Cai;
                    rangoAutorizado = $"{caiActivo.RangoInicialFormateado} al {caiActivo.RangoFinalFormateado}";
                    fechaLimite = caiActivo.FechaLimiteEmision;
                    caiId = caiActivo.Id;

                    caiActivo.CorrelativoActual += 1;
                }
                else if (string.IsNullOrWhiteSpace(numeroRecibo))
                {
                    var count = await _context.Recibos.CountAsync(r => r.FechaEmision.Year == anioActual) + 1;
                    numeroRecibo = $"FAC-{anioActual}-{count:D4}";
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(numeroRecibo))
                {
                    var count = await _context.Recibos.CountAsync(r => r.FechaEmision.Year == anioActual) + 1;
                    numeroRecibo = $"REC-{anioActual}-{count:D4}";
                }
            }

            // Validar / Calcular Totales e Ítems
            var items = dto.Items ?? new List<ReciboItemDto>();
            if (items.Count == 0)
            {
                items.Add(new ReciboItemDto
                {
                    Producto = "Honorarios Profesionales",
                    Descripcion = dto.Concepto ?? "Asesoría Contable y Fiscal",
                    Cantidad = 1,
                    Precio = dto.Monto > 0 ? dto.Monto : dto.Subtotal,
                    Total = dto.Monto > 0 ? dto.Monto : dto.Subtotal
                });
            }

            decimal subtotal = dto.Subtotal > 0 ? dto.Subtotal : items.Sum(i => i.Total);
            decimal impuesto = dto.Impuesto >= 0 ? dto.Impuesto : 0;
            decimal totalFinal = dto.Monto > 0 ? dto.Monto : (subtotal + impuesto);

            int? pagoId = null;
            if (dto.RegistrarComoPago && cliente != null)
            {
                var pago = new PagoHonorario
                {
                    ClienteId = cliente.Id,
                    Monto = totalFinal,
                    FechaPago = dto.FechaEmision.ToUniversalTime(),
                    MetodoPago = dto.MetodoPago ?? "Transferencia",
                    MesAplicado = !string.IsNullOrWhiteSpace(dto.MesAplicado) ? dto.MesAplicado.Trim() : $"{DateTime.UtcNow:MMMM yyyy}",
                    Estado = "Completado",
                    Observaciones = dto.Observaciones?.Trim()
                };
                await _context.PagosHonorarios.AddAsync(pago);
                await _context.SaveChangesAsync();
                pagoId = pago.Id;
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var itemsJson = JsonSerializer.Serialize(items, jsonOptions);

            var conceptoPrincipal = !string.IsNullOrWhiteSpace(dto.Concepto)
                ? dto.Concepto.Trim()
                : (items.FirstOrDefault()?.Producto ?? "Honorarios Contables");

            var recibo = new Recibo
            {
                PagoHonorarioId = pagoId,
                ClienteId = cliente?.Id,
                TipoComprobante = esConCai ? "ConCAI" : "SinCAI",
                NumeroRecibo = numeroRecibo,
                NumeroFiscal = numeroFiscal,
                AutorizacionCAIId = caiId,
                Cai = caiCodigo,
                RangoAutorizado = rangoAutorizado,
                FechaLimiteEmision = fechaLimite,
                FechaEmision = dto.FechaEmision.ToUniversalTime(),
                Concepto = conceptoPrincipal,
                Subtotal = subtotal,
                Impuesto = impuesto,
                Monto = totalFinal,
                MontoEnLetras = NumeroALetras.Convertir(totalFinal),
                NombreCliente = nombreCliente,
                RtnCliente = rtnCliente,
                MetodoPago = dto.MetodoPago,
                MotivoAnulacion = dto.Observaciones,
                ItemsJson = itemsJson,
                Anulado = false
            };

            await _context.Recibos.AddAsync(recibo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReciboById), new { id = recibo.Id }, MapToResponseDto(recibo));
        }

        [HttpPost("{id:int}/anular")]
        public async Task<IActionResult> AnularRecibo(int id, [FromBody] string? motivo)
        {
            var recibo = await _context.Recibos.FindAsync(id);
            if (recibo == null) return NotFound(new { mensaje = "Recibo no encontrado." });

            recibo.Anulado = true;
            recibo.MotivoAnulacion = motivo ?? "Anulado por el usuario";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Comprobante anulado correctamente." });
        }

        [HttpGet("{id:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarReciboPdf(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var recibo = await _context.Recibos
                .Include(r => r.PagoHonorario)
                .ThenInclude(p => p != null ? p.Cliente : null)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recibo == null)
            {
                return NotFound(new { mensaje = "Recibo no encontrado." });
            }

            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            var documento = new ReciboDocument(recibo, config);
            var pdfBytes = documento.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"{recibo.NumeroRecibo}.pdf");
        }

        private static ReciboResponseDto MapToResponseDto(Recibo r)
        {
            var items = new List<ReciboItemDto>();
            if (!string.IsNullOrWhiteSpace(r.ItemsJson))
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    items = JsonSerializer.Deserialize<List<ReciboItemDto>>(r.ItemsJson, jsonOptions) ?? new();
                }
                catch
                {
                    items = new();
                }
            }

            if (items.Count == 0)
            {
                items.Add(new ReciboItemDto
                {
                    Producto = r.Concepto,
                    Descripcion = r.Concepto,
                    Cantidad = 1,
                    Precio = r.Monto,
                    Total = r.Monto
                });
            }

            return new ReciboResponseDto
            {
                Id = r.Id,
                PagoHonorarioId = r.PagoHonorarioId,
                ClienteId = r.ClienteId,
                NombreCliente = r.NombreCliente ?? r.Cliente?.NombreRazonSocial ?? "Cliente General",
                RtnCliente = r.RtnCliente ?? r.Cliente?.Rtn ?? "N/A",
                TipoComprobante = r.TipoComprobante ?? (string.IsNullOrWhiteSpace(r.Cai) ? "SinCAI" : "ConCAI"),
                NumeroRecibo = r.NumeroRecibo,
                NumeroFiscal = r.NumeroFiscal,
                Cai = r.Cai,
                RangoAutorizado = r.RangoAutorizado,
                FechaLimiteEmision = r.FechaLimiteEmision,
                FechaEmision = r.FechaEmision,
                Concepto = r.Concepto,
                Subtotal = r.Subtotal > 0 ? r.Subtotal : r.Monto,
                Impuesto = r.Impuesto,
                Monto = r.Monto,
                MontoEnLetras = r.MontoEnLetras,
                MetodoPago = r.MetodoPago ?? r.PagoHonorario?.MetodoPago,
                Anulado = r.Anulado,
                MotivoAnulacion = r.MotivoAnulacion,
                Items = items
            };
        }
    }
}
