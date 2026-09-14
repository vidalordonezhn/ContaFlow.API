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
                ClienteContrasenaSAR = cliente.ContrasenaSAR,
                CuotaHonorarios = cliente.CuotaMensual,
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

        public async Task<LibroDetalleCompletoDto> GetLibroDetalleCompletoAsync(int clienteId, int anio, int mes)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var periodo = await _context.PeriodosFiscalesSAR
                .Include(p => p.DetalleItems)
                .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.Anio == anio && p.Mes == mes);

            if (periodo == null)
            {
                periodo = new PeriodoFiscalSAR
                {
                    ClienteId = clienteId,
                    Anio = anio,
                    Mes = mes,
                    Estado = "Pendiente"
                };
                _context.PeriodosFiscalesSAR.Add(periodo);
                await _context.SaveChangesAsync();
            }

            var culture = new CultureInfo("es-HN");
            var nombreMes = culture.DateTimeFormat.GetMonthName(mes);
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);

            var items = (periodo.DetalleItems ?? new List<LibroDetalleItem>())
                .OrderBy(d => d.Correlativo)
                .Select(d => new LibroPartidaItemDto
                {
                    Id = d.Id,
                    Correlativo = d.Correlativo,
                    Fecha = d.Fecha?.ToString("yyyy-MM-dd"),
                    Proveedor = d.Proveedor,
                    ComprasExentas = d.ComprasExentas,
                    ComprasGravadas = d.ComprasGravadas,
                    IsvCompras15 = d.IsvCompras15 > 0 ? d.IsvCompras15 : Math.Round(d.ComprasGravadas * 0.15m, 2),
                    FacturaNumero = d.FacturaNumero,
                    VentasExentas = d.VentasExentas,
                    VentasGravadas = d.VentasGravadas,
                    IsvVentas15 = d.IsvVentas15 > 0 ? d.IsvVentas15 : Math.Round(d.VentasGravadas * 0.15m, 2),
                    Notas = d.Notas
                })
                .ToList();

            var totalComprasExentas = items.Sum(i => i.ComprasExentas);
            var totalComprasGravadas = items.Sum(i => i.ComprasGravadas);
            var totalIsvCompras15 = items.Sum(i => i.IsvCompras15);

            var totalVentasExentas = items.Sum(i => i.VentasExentas);
            var totalVentasGravadas = items.Sum(i => i.VentasGravadas);
            var totalIsvVentas15 = items.Sum(i => i.IsvVentas15);

            return new LibroDetalleCompletoDto
            {
                PeriodoFiscalId = periodo.Id,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.NombreRazonSocial,
                ClienteRtn = cliente.Rtn,
                ClienteContrasenaSAR = cliente.ContrasenaSAR,
                CuotaHonorarios = cliente.CuotaMensual,
                Mes = mes,
                Anio = anio,
                MesNombre = $"{nombreMes}, {anio}",
                Items = items,
                TotalComprasExentas = totalComprasExentas,
                TotalComprasGravadas = totalComprasGravadas,
                TotalIsvCompras15 = totalIsvCompras15,
                TotalVentasExentas = totalVentasExentas,
                TotalVentasGravadas = totalVentasGravadas,
                TotalIsvVentas15 = totalIsvVentas15,
                ServiciosProfesionales = cliente.CuotaMensual
            };
        }

        public async Task<LibroDetalleCompletoDto> GuardarLibroDetallePartidasAsync(GuardarLibroDetallePartidasRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(request.ClienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var periodo = await _context.PeriodosFiscalesSAR
                .Include(p => p.DetalleItems)
                .FirstOrDefaultAsync(p => p.ClienteId == request.ClienteId && p.Anio == request.Anio && p.Mes == request.Mes);

            if (periodo == null)
            {
                periodo = new PeriodoFiscalSAR
                {
                    ClienteId = request.ClienteId,
                    Anio = request.Anio,
                    Mes = request.Mes,
                    Estado = "Pendiente"
                };
                _context.PeriodosFiscalesSAR.Add(periodo);
                await _context.SaveChangesAsync();
            }

            // Eliminar items anteriores y reinsertar
            _context.LibroDetalleItems.RemoveRange(periodo.DetalleItems);

            int correlativo = 1;
            var nuevosItems = new List<LibroDetalleItem>();

            foreach (var itemDto in request.Items)
            {
                DateTime? fecha = null;
                if (!string.IsNullOrWhiteSpace(itemDto.Fecha) && DateTime.TryParse(itemDto.Fecha, out var parsedDate))
                {
                    fecha = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }

                var comprasGrav = Math.Round(itemDto.ComprasGravadas, 2);
                var comprasExentas = Math.Round(itemDto.ComprasExentas, 2);
                var isvCompras = Math.Round(comprasGrav * 0.15m, 2);

                var ventasGrav = Math.Round(itemDto.VentasGravadas, 2);
                var ventasExentas = Math.Round(itemDto.VentasExentas, 2);
                var isvVentas = Math.Round(ventasGrav * 0.15m, 2);

                nuevosItems.Add(new LibroDetalleItem
                {
                    PeriodoFiscalId = periodo.Id,
                    Correlativo = correlativo++,
                    Fecha = fecha,
                    Proveedor = itemDto.Proveedor?.Trim(),
                    ComprasExentas = comprasExentas,
                    ComprasGravadas = comprasGrav,
                    IsvCompras15 = isvCompras,
                    FacturaNumero = itemDto.FacturaNumero?.Trim(),
                    VentasExentas = ventasExentas,
                    VentasGravadas = ventasGrav,
                    IsvVentas15 = isvVentas,
                    Notas = itemDto.Notas?.Trim()
                });
            }

            await _context.LibroDetalleItems.AddRangeAsync(nuevosItems);

            // Sincronizar totales en el periodo fiscal
            periodo.ComprasExentas = nuevosItems.Sum(x => x.ComprasExentas);
            periodo.ComprasGravadas15 = nuevosItems.Sum(x => x.ComprasGravadas);
            periodo.IsvCredito15 = nuevosItems.Sum(x => x.IsvCompras15);
            periodo.TotalCreditoFiscal = periodo.IsvCredito15;

            periodo.VentasExentas = nuevosItems.Sum(x => x.VentasExentas);
            periodo.VentasGravadas15 = nuevosItems.Sum(x => x.VentasGravadas);
            periodo.IsvDebito15 = nuevosItems.Sum(x => x.IsvVentas15);
            periodo.TotalDebitoFiscal = periodo.IsvDebito15;

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
            if (nuevosItems.Count > 0 && !periodo.LiquidadoSAR)
            {
                periodo.Estado = "EnProceso";
            }

            await _context.SaveChangesAsync();

            return await GetLibroDetalleCompletoAsync(request.ClienteId, request.Anio, request.Mes);
        }

        public byte[] GenerarPlantillaDetalleCsv()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Correlativo,Fecha,Proveedor,ComprasExentas,ComprasGravadas,FacturaNumero,VentasExentas,VentasGravadas,Notas");
            csv.AppendLine("1,2026-08-01,BOMBOSTORE,0.00,1699.00,001033,0.00,1800.00,");
            csv.AppendLine("2,2026-08-02,BURGER KING,0.00,251.30,001034,0.00,1300.00,");
            csv.AppendLine("3,2026-08-03,TALLER Y AUTO PARTES,0.00,348.50,001035,0.00,600.00,");
            csv.AppendLine("4,2026-08-04,VARIEDADES BICO,0.00,421.74,001036,0.00,0.00,");
            csv.AppendLine("5,2026-08-05,PAGSA,2790.00,0.00,001040,0.00,7500.00,");
            csv.AppendLine("6,2026-08-06,ALINSA,0.00,68.70,001042,600.00,0.00,");

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        }

        public async Task<LibroDetalleCompletoDto> ImportarDetallePartidasCsvAsync(int clienteId, int anio, int mes, string csvContent)
        {
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
            {
                throw new InvalidOperationException("El archivo CSV no contiene filas de datos.");
            }

            var items = new List<LibroPartidaItemDto>();
            int seq = 1;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = SplitCsvLine(line);
                if (cols.Count < 3) continue;

                var fechaStr = cols.Count > 1 ? cols[1].Trim() : "";
                var provStr = cols.Count > 2 ? cols[2].Trim() : "";

                decimal.TryParse(cols.Count > 3 ? cols[3].Trim() : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var comprasEx);
                decimal.TryParse(cols.Count > 4 ? cols[4].Trim() : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var comprasGrav);
                var facturaNum = cols.Count > 5 ? cols[5].Trim() : "";
                decimal.TryParse(cols.Count > 6 ? cols[6].Trim() : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var ventasEx);
                decimal.TryParse(cols.Count > 7 ? cols[7].Trim() : "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var ventasGrav);
                var notas = cols.Count > 8 ? cols[8].Trim() : "";

                items.Add(new LibroPartidaItemDto
                {
                    Correlativo = seq++,
                    Fecha = fechaStr,
                    Proveedor = provStr,
                    ComprasExentas = comprasEx,
                    ComprasGravadas = comprasGrav,
                    IsvCompras15 = Math.Round(comprasGrav * 0.15m, 2),
                    FacturaNumero = facturaNum,
                    VentasExentas = ventasEx,
                    VentasGravadas = ventasGrav,
                    IsvVentas15 = Math.Round(ventasGrav * 0.15m, 2),
                    Notas = notas
                });
            }

            var saveRequest = new GuardarLibroDetallePartidasRequest
            {
                ClienteId = clienteId,
                Anio = anio,
                Mes = mes,
                Items = items
            };

            return await GuardarLibroDetallePartidasAsync(saveRequest);
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result;
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

        // ==========================================
        // === GESTIÓN DUAL OFICIAL: VENTAS Y COMPRAS
        // ==========================================

        public async Task<LibroCompletoMensualDto> GetLibroCompletoAsync(int clienteId, int anio, int mes)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var periodo = await _context.PeriodosFiscalesSAR
                .Include(p => p.VentasDetalleItems)
                .Include(p => p.ComprasDetalleItems)
                .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.Anio == anio && p.Mes == mes);

            if (periodo == null)
            {
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
                    ServiciosProfesionales = cliente.CuotaMensual,
                    Estado = "Pendiente"
                };

                _context.PeriodosFiscalesSAR.Add(periodo);
                await _context.SaveChangesAsync();
            }

            var culture = new CultureInfo("es-HN");
            var nombreMes = culture.DateTimeFormat.GetMonthName(mes);
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);

            // 1. Mapeo Ventas
            var ventasItems = (periodo.VentasDetalleItems ?? new List<LibroVentaDetalleItem>())
                .OrderBy(v => v.Correlativo)
                .Select(v => {
                    var isv15 = v.Isv15 > 0 ? v.Isv15 : Math.Round(v.Gravado15 * 0.15m, 2);
                    var isv18 = v.Isv18 > 0 ? v.Isv18 : Math.Round(v.Gravado18 * 0.18m, 2);
                    var total = v.Total > 0 ? v.Total : (v.Exonerado + v.Exento + v.Gravado15 + v.Gravado18 + isv15 + isv18);
                    return new LibroVentaItemDto
                    {
                        Id = v.Id,
                        Correlativo = v.Correlativo,
                        Fecha = v.Fecha?.ToString("yyyy-MM-dd"),
                        Factura = v.Factura,
                        Exonerado = v.Exonerado,
                        Exento = v.Exento,
                        Gravado15 = v.Gravado15,
                        Gravado18 = v.Gravado18,
                        Isv15 = isv15,
                        Isv18 = isv18,
                        Total = total,
                        Notas = v.Notas
                    };
                }).ToList();

            // 2. Mapeo Compras
            var comprasItems = (periodo.ComprasDetalleItems ?? new List<LibroCompraDetalleItem>())
                .OrderBy(c => c.Correlativo)
                .Select(c => {
                    var isv15 = c.Isv15 > 0 ? c.Isv15 : Math.Round(c.Gravado15 * 0.15m, 2);
                    var isv18 = c.Isv18 > 0 ? c.Isv18 : Math.Round(c.Gravado18 * 0.18m, 2);
                    var total = c.Total > 0 ? c.Total : (c.Exonerado + c.Exento + c.Gravado15 + c.Gravado18 + isv15 + isv18);
                    return new LibroCompraItemDto
                    {
                        Id = c.Id,
                        Correlativo = c.Correlativo,
                        Fecha = c.Fecha?.ToString("yyyy-MM-dd"),
                        Factura = c.Factura,
                        Proveedor = c.Proveedor,
                        Exonerado = c.Exonerado,
                        Exento = c.Exento,
                        Gravado15 = c.Gravado15,
                        Gravado18 = c.Gravado18,
                        Isv15 = isv15,
                        Isv18 = isv18,
                        Total = total,
                        Notas = c.Notas
                    };
                }).ToList();

            // 3. Resúmenes
            var resumenVentas = new ResumenVentasCasillasDto
            {
                TotalExonerado = ventasItems.Sum(v => v.Exonerado),
                TotalExento = ventasItems.Sum(v => v.Exento),
                TotalGravado15 = ventasItems.Sum(v => v.Gravado15),
                TotalGravado18 = ventasItems.Sum(v => v.Gravado18),
                TotalIsv15 = ventasItems.Sum(v => v.Isv15),
                TotalIsv18 = ventasItems.Sum(v => v.Isv18)
            };

            var resumenCompras = new ResumenComprasCasillasDto
            {
                TotalExonerado = comprasItems.Sum(c => c.Exonerado),
                TotalExento = comprasItems.Sum(c => c.Exento),
                TotalGravado15 = comprasItems.Sum(c => c.Gravado15),
                TotalGravado18 = comprasItems.Sum(c => c.Gravado18),
                TotalIsv15 = comprasItems.Sum(c => c.Isv15),
                TotalIsv18 = comprasItems.Sum(c => c.Isv18)
            };

            // 4. Liquidación Consolidada
            var honorarios = periodo.ServiciosProfesionales > 0 ? periodo.ServiciosProfesionales : cliente.CuotaMensual;
            var ret15 = periodo.Retenciones15;
            var ret18 = periodo.Retenciones18;
            var saldoAnt = periodo.SaldoAFavorPeriodoAnterior;

            var debito = resumenVentas.TotalDebitoFiscal;
            var credito = resumenCompras.TotalCreditoFiscal;
            var liquidacionNeta = debito - credito - saldoAnt - ret15 - ret18;

            var liquidacionPagar = liquidacionNeta > 0 ? Math.Round(liquidacionNeta, 2) : 0;
            var saldoFavorContribuyente = liquidacionNeta < 0 ? Math.Round(Math.Abs(liquidacionNeta), 2) : 0;
            var totalLps = liquidacionPagar + honorarios;

            var liquidacion = new LiquidacionConsolidadaDto
            {
                DebitoFiscalVentas = debito,
                CreditoFiscalCompras = credito,
                SaldoAFavorPeriodoAnterior = saldoAnt,
                Retenciones15 = ret15,
                Retenciones18 = ret18,
                LiquidacionFinalPagar = liquidacionPagar,
                SaldoAFavorContribuyente = saldoFavorContribuyente,
                ServiciosProfesionales = honorarios,
                TotalPagarLps = totalLps
            };

            return new LibroCompletoMensualDto
            {
                PeriodoFiscalId = periodo.Id,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.NombreRazonSocial,
                ClienteRtn = cliente.Rtn,
                ClienteContrasenaSAR = cliente.ContrasenaSAR,
                CuotaHonorarios = cliente.CuotaMensual,
                Mes = mes,
                Anio = anio,
                MesNombre = $"{nombreMes}, {anio}",
                VentasItems = ventasItems,
                ComprasItems = comprasItems,
                ResumenVentas = resumenVentas,
                ResumenCompras = resumenCompras,
                Liquidacion = liquidacion,
                LiquidadoSAR = periodo.LiquidadoSAR,
                FechaLiquidacion = periodo.FechaLiquidacion,
                NumeroDeclaracionSAR = periodo.NumeroDeclaracionSAR,
                Estado = periodo.Estado
            };
        }

        public async Task<LibroCompletoMensualDto> GuardarLibroCompletoAsync(GuardarLibroCompletoRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(request.ClienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            var periodo = await _context.PeriodosFiscalesSAR
                .Include(p => p.VentasDetalleItems)
                .Include(p => p.ComprasDetalleItems)
                .FirstOrDefaultAsync(p => p.ClienteId == request.ClienteId && p.Anio == request.Anio && p.Mes == request.Mes);

            if (periodo == null)
            {
                periodo = new PeriodoFiscalSAR
                {
                    ClienteId = request.ClienteId,
                    Anio = request.Anio,
                    Mes = request.Mes,
                    Estado = "Pendiente"
                };
                _context.PeriodosFiscalesSAR.Add(periodo);
                await _context.SaveChangesAsync();
            }

            // 1. Limpiar e insertar partidas de Ventas
            if (periodo.VentasDetalleItems != null && periodo.VentasDetalleItems.Count > 0)
            {
                _context.LibrosVentasItems.RemoveRange(periodo.VentasDetalleItems);
            }

            int seqVentas = 1;
            var nuevasVentas = new List<LibroVentaDetalleItem>();
            foreach (var item in request.VentasItems)
            {
                DateTime? fecha = null;
                if (!string.IsNullOrWhiteSpace(item.Fecha) && DateTime.TryParse(item.Fecha, out var parsedDate))
                {
                    fecha = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }

                var grav15 = Math.Round(item.Gravado15, 2);
                var grav18 = Math.Round(item.Gravado18, 2);
                var isv15 = Math.Round(grav15 * 0.15m, 2);
                var isv18 = Math.Round(grav18 * 0.18m, 2);
                var exon = Math.Round(item.Exonerado, 2);
                var exen = Math.Round(item.Exento, 2);
                var tot = exon + exen + grav15 + grav18 + isv15 + isv18;

                nuevasVentas.Add(new LibroVentaDetalleItem
                {
                    PeriodoFiscalId = periodo.Id,
                    ClienteId = request.ClienteId,
                    Correlativo = seqVentas++,
                    Fecha = fecha,
                    Factura = item.Factura?.Trim(),
                    Exonerado = exon,
                    Exento = exen,
                    Gravado15 = grav15,
                    Gravado18 = grav18,
                    Isv15 = isv15,
                    Isv18 = isv18,
                    Total = tot,
                    Notas = item.Notas?.Trim()
                });
            }
            await _context.LibrosVentasItems.AddRangeAsync(nuevasVentas);

            // 2. Limpiar e insertar partidas de Compras
            if (periodo.ComprasDetalleItems != null && periodo.ComprasDetalleItems.Count > 0)
            {
                _context.LibrosComprasItems.RemoveRange(periodo.ComprasDetalleItems);
            }

            int seqCompras = 1;
            var nuevasCompras = new List<LibroCompraDetalleItem>();
            foreach (var item in request.ComprasItems)
            {
                DateTime? fecha = null;
                if (!string.IsNullOrWhiteSpace(item.Fecha) && DateTime.TryParse(item.Fecha, out var parsedDate))
                {
                    fecha = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }

                var grav15 = Math.Round(item.Gravado15, 2);
                var grav18 = Math.Round(item.Gravado18, 2);
                var isv15 = Math.Round(grav15 * 0.15m, 2);
                var isv18 = Math.Round(grav18 * 0.18m, 2);
                var exon = Math.Round(item.Exonerado, 2);
                var exen = Math.Round(item.Exento, 2);
                var tot = exon + exen + grav15 + grav18 + isv15 + isv18;

                nuevasCompras.Add(new LibroCompraDetalleItem
                {
                    PeriodoFiscalId = periodo.Id,
                    ClienteId = request.ClienteId,
                    Correlativo = seqCompras++,
                    Fecha = fecha,
                    Factura = item.Factura?.Trim(),
                    Proveedor = item.Proveedor?.Trim(),
                    Exonerado = exon,
                    Exento = exen,
                    Gravado15 = grav15,
                    Gravado18 = grav18,
                    Isv15 = isv15,
                    Isv18 = isv18,
                    Total = tot,
                    Notas = item.Notas?.Trim()
                });
            }
            await _context.LibrosComprasItems.AddRangeAsync(nuevasCompras);

            // 3. Sincronizar totales agregados del periodo fiscal
            periodo.VentasExentas = nuevasVentas.Sum(v => v.Exento + v.Exonerado);
            periodo.VentasGravadas15 = nuevasVentas.Sum(v => v.Gravado15);
            periodo.VentasGravadas18 = nuevasVentas.Sum(v => v.Gravado18);
            periodo.IsvDebito15 = nuevasVentas.Sum(v => v.Isv15);
            periodo.IsvDebito18 = nuevasVentas.Sum(v => v.Isv18);
            periodo.TotalDebitoFiscal = periodo.IsvDebito15 + periodo.IsvDebito18;

            periodo.ComprasExentas = nuevasCompras.Sum(c => c.Exento + c.Exonerado);
            periodo.ComprasGravadas15 = nuevasCompras.Sum(c => c.Gravado15);
            periodo.ComprasGravadas18 = nuevasCompras.Sum(c => c.Gravado18);
            periodo.IsvCredito15 = nuevasCompras.Sum(c => c.Isv15);
            periodo.IsvCredito18 = nuevasCompras.Sum(c => c.Isv18);
            periodo.TotalCreditoFiscal = periodo.IsvCredito15 + periodo.IsvCredito18;

            periodo.SaldoAFavorPeriodoAnterior = request.SaldoAFavorPeriodoAnterior;
            periodo.Retenciones15 = request.Retenciones15;
            periodo.Retenciones18 = request.Retenciones18;
            periodo.RetencionesISVRecibidas = request.Retenciones15 + request.Retenciones18;
            periodo.ServiciosProfesionales = request.ServiciosProfesionales ?? cliente.CuotaMensual;

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

            var cantVentas = nuevasVentas.Count(v => !string.IsNullOrWhiteSpace(v.Factura) || v.Total > 0 || v.Gravado15 > 0 || v.Gravado18 > 0 || v.Exento > 0 || v.Exonerado > 0);
            var cantCompras = nuevasCompras.Count(c => !string.IsNullOrWhiteSpace(c.Factura) || !string.IsNullOrWhiteSpace(c.Proveedor) || c.Total > 0 || c.Gravado15 > 0 || c.Gravado18 > 0 || c.Exento > 0 || c.Exonerado > 0);
            var hayFacturas = cantVentas > 0 || cantCompras > 0;

            periodo.CantidadFacturasVenta = cantVentas;
            periodo.CantidadFacturasCompra = cantCompras;
            periodo.FacturasRecibidas = hayFacturas;
            if (hayFacturas && periodo.FechaRecepcionFacturas == null)
            {
                periodo.FechaRecepcionFacturas = DateTime.UtcNow;
            }
            else if (!hayFacturas)
            {
                periodo.FechaRecepcionFacturas = null;
            }

            if (request.MarcarComoLiquidado || !string.IsNullOrWhiteSpace(request.NumeroDeclaracionSAR))
            {
                periodo.LiquidadoSAR = true;
                periodo.FechaLiquidacion = DateTime.UtcNow;
                periodo.NumeroDeclaracionSAR = request.NumeroDeclaracionSAR?.Trim();
                periodo.Estado = "Declarado";
            }
            else if (hayFacturas)
            {
                periodo.Estado = "EnProceso";
            }
            else
            {
                periodo.Estado = "Pendiente";
            }

            await _context.SaveChangesAsync();

            return await GetLibroCompletoAsync(request.ClienteId, request.Anio, request.Mes);
        }
    }
}
