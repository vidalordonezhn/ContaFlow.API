using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Pagos.DTOs;
using ContaFlow.API.Features.Recibos;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.Pagos
{
    public class PagosService
    {
        private readonly ContaFlowDbContext _context;

        public PagosService(ContaFlowDbContext context)
        {
            _context = context;
        }

        public async Task<List<PagoResponseDto>> GetPagosAsync()
        {
            return await _context.PagosHonorarios
                .Include(p => p.Cliente)
                .Include(p => p.Recibo)
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new PagoResponseDto
                {
                    Id = p.Id,
                    ClienteId = p.ClienteId,
                    ClienteNombre = p.Cliente.NombreRazonSocial,
                    ClienteRtn = p.Cliente.Rtn,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    MetodoPago = p.MetodoPago,
                    ReferenciaBancaria = p.ReferenciaBancaria,
                    MesAplicado = p.MesAplicado,
                    Estado = p.Estado,
                    Observaciones = p.Observaciones,
                    ReciboId = p.Recibo != null ? p.Recibo.Id : null,
                    NumeroRecibo = p.Recibo != null ? p.Recibo.NumeroRecibo : null
                })
                .ToListAsync();
        }

        public async Task<PagoResponseDto> RegistrarPagoAsync(PagoCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var pago = new PagoHonorario
            {
                ClienteId = dto.ClienteId,
                Monto = dto.Monto,
                FechaPago = dto.FechaPago.ToUniversalTime(),
                MetodoPago = dto.MetodoPago,
                ReferenciaBancaria = dto.ReferenciaBancaria?.Trim(),
                MesAplicado = dto.MesAplicado.Trim(),
                Estado = "Completado",
                Observaciones = dto.Observaciones?.Trim()
            };

            await _context.PagosHonorarios.AddAsync(pago);
            await _context.SaveChangesAsync();

            if (dto.GenerarRecibo)
            {
                var anioActual = DateTime.UtcNow.Year;
                var prefix = $"REC-{anioActual}-";
                var maxExistente = await _context.Recibos
                    .Where(r => r.NumeroRecibo.StartsWith(prefix))
                    .Select(r => r.NumeroRecibo)
                    .ToListAsync();

                int nextNum = 1;
                if (maxExistente.Count > 0)
                {
                    var numeros = maxExistente
                        .Select(nr => nr.Substring(prefix.Length))
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse);
                    if (numeros.Any())
                    {
                        nextNum = numeros.Max() + 1;
                    }
                }
                var numeroInterno = $"{prefix}{nextNum:D4}";

                // Buscar autorización CAI activa y con correlativos disponibles
                var caiActivo = await _context.AutorizacionesCAI
                    .Where(c => c.Activo && c.CorrelativoActual <= c.RangoFinal && c.FechaLimiteEmision >= DateTime.UtcNow)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                string numeroRecibo = numeroInterno;
                string? numeroFiscal = null;
                string? caiCodigo = null;
                string? rangoAutorizado = null;
                DateTime? fechaLimite = null;
                int? caiId = null;

                if (caiActivo != null)
                {
                    numeroFiscal = caiActivo.SiguienteNumeroFormateado;
                    numeroRecibo = numeroFiscal;
                    caiCodigo = caiActivo.Cai;
                    rangoAutorizado = $"{caiActivo.RangoInicialFormateado} al {caiActivo.RangoFinalFormateado}";
                    fechaLimite = caiActivo.FechaLimiteEmision;
                    caiId = caiActivo.Id;

                    // Consumir el correlativo de forma atómica
                    caiActivo.CorrelativoActual += 1;
                }

                // Garantizar unicidad absoluta
                if (await _context.Recibos.AnyAsync(r => r.NumeroRecibo == numeroRecibo))
                {
                    var baseNum = numeroRecibo;
                    var sufijo = 2;
                    while (await _context.Recibos.AnyAsync(r => r.NumeroRecibo == $"{baseNum}-{sufijo}"))
                    {
                        sufijo++;
                    }
                    numeroRecibo = $"{baseNum}-{sufijo}";
                }

                var recibo = new Recibo
                {
                    PagoHonorarioId = pago.Id,
                    NumeroRecibo = numeroRecibo,
                    NumeroFiscal = numeroFiscal,
                    AutorizacionCAIId = caiId,
                    Cai = caiCodigo,
                    RangoAutorizado = rangoAutorizado,
                    FechaLimiteEmision = fechaLimite,
                    FechaEmision = DateTime.UtcNow,
                    Concepto = $"Honorarios Contables y Asesoría Fiscal - {dto.MesAplicado}",
                    Monto = dto.Monto,
                    MontoEnLetras = NumeroALetras.Convertir(dto.Monto),
                    NombreCliente = cliente.NombreRazonSocial,
                    RtnCliente = cliente.Rtn,
                    Anulado = false
                };

                await _context.Recibos.AddAsync(recibo);
                await _context.SaveChangesAsync();
            }

            return (await GetPagosAsync()).First(p => p.Id == pago.Id);
        }
    }
}
