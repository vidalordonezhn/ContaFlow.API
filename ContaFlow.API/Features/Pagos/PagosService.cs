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
                var conteoRecibos = await _context.Recibos.CountAsync(r => r.FechaEmision.Year == anioActual) + 1;
                var numeroInterno = $"REC-{anioActual}-{conteoRecibos:D4}";

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
