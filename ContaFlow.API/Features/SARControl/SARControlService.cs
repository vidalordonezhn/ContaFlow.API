using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.SARControl.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.SARControl
{
    public class SARControlService
    {
        private readonly ContaFlowDbContext _context;

        public SARControlService(ContaFlowDbContext context)
        {
            _context = context;
        }

        public async Task<List<PeriodoSARResponseDto>> GetPeriodosPorMesAsync(int mes, int anio)
        {
            // Asegurar que todos los clientes activos tengan su registro de período para este mes
            await SincronizarPeriodosMensualesAsync(mes, anio);

            var periodos = await _context.PeriodosFiscalesSAR
                .Include(p => p.Cliente)
                .Where(p => p.Mes == mes && p.Anio == anio && p.Cliente.Activo)
                .OrderBy(p => p.Cliente.NombreRazonSocial)
                .ToListAsync();

            var culture = new CultureInfo("es-HN");

            return periodos.Select(p =>
            {
                var semaforo = "Rojo";
                if (p.LiquidadoSAR)
                    semaforo = "Verde";
                else if (p.FacturasRecibidas)
                    semaforo = "Amarillo";

                var nombreMes = culture.DateTimeFormat.GetMonthName(p.Mes);
                nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);

                return new PeriodoSARResponseDto
                {
                    Id = p.Id,
                    ClienteId = p.ClienteId,
                    ClienteNombre = p.Cliente.NombreRazonSocial,
                    ClienteRtn = p.Cliente.Rtn,
                    ClienteRubro = p.Cliente.Rubro,
                    ClienteWhatsApp = p.Cliente.TelefonoWhatsApp,
                    ClienteEmail = p.Cliente.EmailPrincipal,
                    Mes = p.Mes,
                    Anio = p.Anio,
                    MesNombre = $"{nombreMes} {p.Anio}",
                    FacturasRecibidas = p.FacturasRecibidas,
                    FechaRecepcionFacturas = p.FechaRecepcionFacturas,
                    CantidadFacturasVenta = p.CantidadFacturasVenta,
                    CantidadFacturasCompra = p.CantidadFacturasCompra,
                    NotasDocumentos = p.NotasDocumentos,
                    LiquidadoSAR = p.LiquidadoSAR,
                    FechaLiquidacion = p.FechaLiquidacion,
                    MontoImpuestoISV = p.MontoImpuestoISV,
                    MontoRetenciones = p.MontoRetenciones,
                    NumeroDeclaracionSAR = p.NumeroDeclaracionSAR,
                    Estado = p.Estado,
                    NivelSemaforo = semaforo
                };
            }).ToList();
        }

        public async Task<SARResumenMensualDto> GetResumenMensualAsync(int mes, int anio)
        {
            await SincronizarPeriodosMensualesAsync(mes, anio);

            var periodos = await _context.PeriodosFiscalesSAR
                .Include(p => p.Cliente)
                .Where(p => p.Mes == mes && p.Anio == anio && p.Cliente.Activo)
                .ToListAsync();

            var total = periodos.Count;
            var recibidas = periodos.Count(p => p.FacturasRecibidas);
            var liquidados = periodos.Count(p => p.LiquidadoSAR);
            var pendientes = total - recibidas;

            var ahora = DateTime.UtcNow;
            var fechaLimiteDia10 = new DateTime(anio, mes, 10, 23, 59, 59, DateTimeKind.Utc);
            var diasRestantes = (int)Math.Ceiling((fechaLimiteDia10 - ahora).TotalDays);

            return new SARResumenMensualDto
            {
                Mes = mes,
                Anio = anio,
                TotalClientesActivos = total,
                FacturasRecibidasCount = recibidas,
                FacturasPendientesCount = pendientes,
                LiquidadosSARCount = liquidados,
                DiasRestantesParaDia10 = diasRestantes > 0 ? diasRestantes : 0,
                AlertaDia10Proximo = diasRestantes <= 5 && diasRestantes >= 0
            };
        }

        public async Task MarcarFacturasRecibidasAsync(int periodoId, MarcarRecepcionDto dto)
        {
            var periodo = await _context.PeriodosFiscalesSAR.FindAsync(periodoId);
            if (periodo == null)
            {
                throw new KeyNotFoundException("Período fiscal no encontrado.");
            }

            periodo.FacturasRecibidas = dto.FacturasRecibidas;
            periodo.FechaRecepcionFacturas = dto.FacturasRecibidas ? DateTime.UtcNow : null;
            periodo.CantidadFacturasVenta = dto.CantidadFacturasVenta;
            periodo.CantidadFacturasCompra = dto.CantidadFacturasCompra;
            periodo.NotasDocumentos = dto.NotasDocumentos;

            if (periodo.FacturasRecibidas && !periodo.LiquidadoSAR)
            {
                periodo.Estado = "EnProceso";
            }
            else if (!periodo.FacturasRecibidas)
            {
                periodo.Estado = "Pendiente";
            }

            await _context.SaveChangesAsync();
        }

        public async Task RegistrarLiquidacionSARAsync(int periodoId, RegistrarLiquidacionSARDto dto)
        {
            var periodo = await _context.PeriodosFiscalesSAR.FindAsync(periodoId);
            if (periodo == null)
            {
                throw new KeyNotFoundException("Período fiscal no encontrado.");
            }

            periodo.LiquidadoSAR = true;
            periodo.FechaLiquidacion = DateTime.UtcNow;
            periodo.NumeroDeclaracionSAR = dto.NumeroDeclaracionSAR.Trim();
            periodo.MontoImpuestoISV = dto.MontoImpuestoISV;
            periodo.MontoRetenciones = dto.MontoRetenciones;
            periodo.Estado = "Declarado";

            await _context.SaveChangesAsync();
        }

        private async Task SincronizarPeriodosMensualesAsync(int mes, int anio)
        {
            var clientesActivos = await _context.Clientes
                .Where(c => c.Activo)
                .ToListAsync();

            var periodosExistentes = await _context.PeriodosFiscalesSAR
                .Where(p => p.Mes == mes && p.Anio == anio)
                .Select(p => p.ClienteId)
                .ToListAsync();

            var nuevosPeriodos = clientesActivos
                .Where(c => !periodosExistentes.Contains(c.Id))
                .Select(c => new PeriodoFiscalSAR
                {
                    ClienteId = c.Id,
                    Mes = mes,
                    Anio = anio,
                    FacturasRecibidas = false,
                    LiquidadoSAR = false,
                    Estado = "Pendiente"
                }).ToList();

            if (nuevosPeriodos.Any())
            {
                await _context.PeriodosFiscalesSAR.AddRangeAsync(nuevosPeriodos);
                await _context.SaveChangesAsync();
            }
        }
    }
}
