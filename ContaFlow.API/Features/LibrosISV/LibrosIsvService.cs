using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.LibrosISV.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.LibrosISV
{
    public class LibrosIsvService
    {
        private readonly ContaFlowDbContext _context;

        public LibrosIsvService(ContaFlowDbContext context)
        {
            _context = context;
        }

        public async Task<LibroIsvDetalleDto> GetLibroIsvAsync(int clienteId, int anio, int mes)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var periodo = await _context.PeriodosFiscalesSAR
                .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.Anio == anio && p.Mes == mes);

            if (periodo == null)
            {
                // Buscar saldo a favor del mes anterior si existe
                var mesAnterior = mes == 1 ? 12 : mes - 1;
                var anioAnterior = mes == 1 ? anio - 1 : anio;
                var periodoAnterior = await _context.PeriodosFiscalesSAR
                    .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.Anio == anioAnterior && p.Mes == mesAnterior);

                var saldoAnterior = periodoAnterior?.SaldoAFavorContribuyente ?? 0;

                periodo = new PeriodoFiscalSAR
                {
                    ClienteId = clienteId,
                    Anio = anio,
                    Mes = mes,
                    SaldoAFavorPeriodoAnterior = saldoAnterior,
                    Estado = "Pendiente"
                };

                _context.PeriodosFiscalesSAR.Add(periodo);
                await _context.SaveChangesAsync();
            }

            var culture = new CultureInfo("es-HN");
            var nombreMes = culture.DateTimeFormat.GetMonthName(mes);
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);

            return new LibroIsvDetalleDto
            {
                Id = periodo.Id,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.NombreRazonSocial,
                ClienteRtn = cliente.Rtn,
                Mes = mes,
                Anio = anio,
                MesNombre = $"{nombreMes} {anio}",
                FacturasRecibidas = periodo.FacturasRecibidas,
                CantidadFacturasVenta = periodo.CantidadFacturasVenta,
                CantidadFacturasCompra = periodo.CantidadFacturasCompra,
                VentasGravadas15 = periodo.VentasGravadas15,
                VentasGravadas18 = periodo.VentasGravadas18,
                VentasExentas = periodo.VentasExentas,
                IsvDebito15 = periodo.IsvDebito15,
                IsvDebito18 = periodo.IsvDebito18,
                TotalDebitoFiscal = periodo.TotalDebitoFiscal,
                ComprasGravadas15 = periodo.ComprasGravadas15,
                ComprasGravadas18 = periodo.ComprasGravadas18,
                ComprasExentas = periodo.ComprasExentas,
                ImportacionesGravadas15 = periodo.ImportacionesGravadas15,
                IsvCredito15 = periodo.IsvCredito15,
                IsvCredito18 = periodo.IsvCredito18,
                TotalCreditoFiscal = periodo.TotalCreditoFiscal,
                SaldoAFavorPeriodoAnterior = periodo.SaldoAFavorPeriodoAnterior,
                RetencionesISVRecibidas = periodo.RetencionesISVRecibidas,
                ImpuestoDeterminadoPagar = periodo.ImpuestoDeterminadoPagar,
                SaldoAFavorContribuyente = periodo.SaldoAFavorContribuyente,
                LiquidadoSAR = periodo.LiquidadoSAR,
                FechaLiquidacion = periodo.FechaLiquidacion,
                NumeroDeclaracionSAR = periodo.NumeroDeclaracionSAR,
                Estado = periodo.Estado
            };
        }

        public async Task<LibroIsvDetalleDto> GuardarLibroIsvAsync(LibroIsvGuardarDto dto)
        {
            var periodo = await _context.PeriodosFiscalesSAR
                .FirstOrDefaultAsync(p => p.ClienteId == dto.ClienteId && p.Anio == dto.Anio && p.Mes == dto.Mes);

            if (periodo == null)
            {
                periodo = new PeriodoFiscalSAR
                {
                    ClienteId = dto.ClienteId,
                    Anio = dto.Anio,
                    Mes = dto.Mes
                };
                _context.PeriodosFiscalesSAR.Add(periodo);
            }

            // Ventas
            periodo.VentasGravadas15 = dto.VentasGravadas15;
            periodo.VentasGravadas18 = dto.VentasGravadas18;
            periodo.VentasExentas = dto.VentasExentas;
            periodo.IsvDebito15 = Math.Round(dto.VentasGravadas15 * 0.15m, 2);
            periodo.IsvDebito18 = Math.Round(dto.VentasGravadas18 * 0.18m, 2);
            periodo.TotalDebitoFiscal = periodo.IsvDebito15 + periodo.IsvDebito18;

            // Compras
            periodo.ComprasGravadas15 = dto.ComprasGravadas15;
            periodo.ComprasGravadas18 = dto.ComprasGravadas18;
            periodo.ComprasExentas = dto.ComprasExentas;
            periodo.ImportacionesGravadas15 = dto.ImportacionesGravadas15;
            periodo.IsvCredito15 = Math.Round((dto.ComprasGravadas15 + dto.ImportacionesGravadas15) * 0.15m, 2);
            periodo.IsvCredito18 = Math.Round(dto.ComprasGravadas18 * 0.18m, 2);
            periodo.TotalCreditoFiscal = periodo.IsvCredito15 + periodo.IsvCredito18;

            // Liquidación SAR-210
            periodo.SaldoAFavorPeriodoAnterior = dto.SaldoAFavorPeriodoAnterior;
            periodo.RetencionesISVRecibidas = dto.RetencionesISVRecibidas;

            var diferencia = periodo.TotalDebitoFiscal - periodo.TotalCreditoFiscal - periodo.SaldoAFavorPeriodoAnterior - periodo.RetencionesISVRecibidas;

            if (diferencia > 0)
            {
                periodo.ImpuestoDeterminadoPagar = Math.Round(diferencia, 2);
                periodo.SaldoAFavorContribuyente = 0;
            }
            else
            {
                periodo.ImpuestoDeterminadoPagar = 0;
                periodo.SaldoAFavorContribuyente = Math.Round(Math.Abs(diferencia), 2);
            }

            periodo.MontoImpuestoISV = periodo.ImpuestoDeterminadoPagar;

            if (dto.MarcarComoLiquidado)
            {
                periodo.LiquidadoSAR = true;
                periodo.FechaLiquidacion = DateTime.UtcNow;
                periodo.NumeroDeclaracionSAR = dto.NumeroDeclaracionSAR?.Trim();
                periodo.Estado = "Declarado";
            }
            else if (!periodo.LiquidadoSAR && (periodo.VentasGravadas15 > 0 || periodo.ComprasGravadas15 > 0))
            {
                periodo.Estado = "EnProceso";
            }

            await _context.SaveChangesAsync();

            return await GetLibroIsvAsync(dto.ClienteId, dto.Anio, dto.Mes);
        }

        public async Task<LibroIsvImportResponseDto> ImportarMasivoAsync(List<LibroIsvImportItemDto> items)
        {
            var response = new LibroIsvImportResponseDto
            {
                TotalProcesados = items.Count
            };

            var clientes = await _context.Clientes.ToListAsync();
            var clientesDict = clientes.ToDictionary(c => c.Rtn.Trim().Replace("-", "").ToUpper(), c => c);

            foreach (var item in items)
            {
                var cleanRtn = (item.Rtn ?? string.Empty).Trim().Replace("-", "").ToUpper();
                if (!clientesDict.TryGetValue(cleanRtn, out var cliente))
                {
                    response.TotalErrores++;
                    response.Mensajes.Add($"RTN no encontrado: {cleanRtn}. Registre el cliente primero.");
                    continue;
                }

                if (item.Mes < 1 || item.Mes > 12 || item.Anio < 2000)
                {
                    response.TotalErrores++;
                    response.Mensajes.Add($"Mes/Año inválido para RTN {cleanRtn} ({item.Mes}/{item.Anio}).");
                    continue;
                }

                var guardarDto = new LibroIsvGuardarDto
                {
                    ClienteId = cliente.Id,
                    Mes = item.Mes,
                    Anio = item.Anio,
                    VentasGravadas15 = item.VentasGravadas15,
                    VentasGravadas18 = item.VentasGravadas18,
                    VentasExentas = item.VentasExentas,
                    ComprasGravadas15 = item.ComprasGravadas15,
                    ComprasGravadas18 = item.ComprasGravadas18,
                    ComprasExentas = item.ComprasExentas,
                    ImportacionesGravadas15 = item.ImportacionesGravadas15,
                    SaldoAFavorPeriodoAnterior = item.SaldoAFavorPeriodoAnterior,
                    RetencionesISVRecibidas = item.RetencionesISVRecibidas,
                    NumeroDeclaracionSAR = item.NumeroDeclaracionSAR,
                    MarcarComoLiquidado = !string.IsNullOrWhiteSpace(item.NumeroDeclaracionSAR)
                };

                await GuardarLibroIsvAsync(guardarDto);
                response.TotalGuardados++;
            }

            return response;
        }

        public byte[] GenerarPlantillaCsv(List<Cliente> clientes)
        {
            var csv = new StringBuilder();
            csv.AppendLine("RTN,NombreCliente,Anio,Mes,VentasGravadas15,VentasGravadas18,VentasExentas,ComprasGravadas15,ComprasGravadas18,ComprasExentas,ImportacionesGravadas15,SaldoAFavorAnterior,RetencionesSufridas,NumeroDeclaracionSAR");

            var ahora = DateTime.UtcNow;
            var anio = ahora.Year;
            var mes = ahora.Month;

            if (clientes.Count == 0)
            {
                csv.AppendLine($"08011990123456,Distribuidora Ejemplo S.A.,{anio},{mes},150000.00,0.00,25000.00,85000.00,0.00,12000.00,0.00,0.00,1500.00,SAR-2026-001");
            }
            else
            {
                foreach (var c in clientes.Take(10))
                {
                    csv.AppendLine($"{c.Rtn},{EscapeCsv(c.NombreRazonSocial)},{anio},{mes},0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,");
                }
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
