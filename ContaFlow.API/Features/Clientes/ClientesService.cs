using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Clientes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.Clientes
{
    public class ClientesService
    {
        private readonly ContaFlowDbContext _context;

        public ClientesService(ContaFlowDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClienteResponseDto>> GetClientesAsync()
        {
            var ahora = DateTime.UtcNow;
            var mesActual = ahora.Month;
            var anioActual = ahora.Year;

            return await _context.Clientes
                .OrderBy(c => c.NombreRazonSocial)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Rtn = c.Rtn,
                    NombreRazonSocial = c.NombreRazonSocial,
                    NombreComercial = c.NombreComercial,
                    TipoPersona = c.TipoPersona,
                    Rubro = c.Rubro,
                    EmailPrincipal = c.EmailPrincipal,
                    EmailSecundario = c.EmailSecundario,
                    Telefono = c.Telefono,
                    TelefonoWhatsApp = c.TelefonoWhatsApp,
                    Direccion = c.Direccion,
                    CuotaMensual = c.CuotaMensual,
                    DiaCobro = c.DiaCobro,
                    ContrasenaSAR = c.ContrasenaSAR,
                    Activo = c.Activo,
                    Notas = c.Notas,
                    FechaCreacion = c.FechaCreacion,
                    TotalPagado = c.Pagos.Where(p => p.Estado == "Completado").Sum(p => p.Monto),
                    PagosRealizadosCount = c.Pagos.Count(p => p.Estado == "Completado"),
                    FacturasMesActualRecibidas = c.PeriodosFiscales.Any(pf => pf.Mes == mesActual && pf.Anio == anioActual && pf.FacturasRecibidas)
                })
                .ToListAsync();
        }

        public async Task<ClienteResponseDto> GetClienteByIdAsync(int id)
        {
            var ahora = DateTime.UtcNow;
            var mesActual = ahora.Month;
            var anioActual = ahora.Year;

            var c = await _context.Clientes
                .Include(cl => cl.Pagos)
                .Include(cl => cl.PeriodosFiscales)
                .FirstOrDefaultAsync(cl => cl.Id == id);

            if (c == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            return new ClienteResponseDto
            {
                Id = c.Id,
                Rtn = c.Rtn,
                NombreRazonSocial = c.NombreRazonSocial,
                NombreComercial = c.NombreComercial,
                TipoPersona = c.TipoPersona,
                Rubro = c.Rubro,
                EmailPrincipal = c.EmailPrincipal,
                EmailSecundario = c.EmailSecundario,
                Telefono = c.Telefono,
                TelefonoWhatsApp = c.TelefonoWhatsApp,
                Direccion = c.Direccion,
                CuotaMensual = c.CuotaMensual,
                DiaCobro = c.DiaCobro,
                ContrasenaSAR = c.ContrasenaSAR,
                Activo = c.Activo,
                Notas = c.Notas,
                FechaCreacion = c.FechaCreacion,
                TotalPagado = c.Pagos.Where(p => p.Estado == "Completado").Sum(p => p.Monto),
                PagosRealizadosCount = c.Pagos.Count(p => p.Estado == "Completado"),
                FacturasMesActualRecibidas = c.PeriodosFiscales.Any(pf => pf.Mes == mesActual && pf.Anio == anioActual && pf.FacturasRecibidas)
            };
        }

        public async Task<ClienteResponseDto> CrearClienteAsync(ClienteCreateDto dto)
        {
            var rtnLimpio = dto.Rtn.Trim().Replace("-", "").Replace(" ", "");

            if (await _context.Clientes.AnyAsync(c => c.Rtn == rtnLimpio))
            {
                throw new InvalidOperationException($"Ya existe un cliente registrado con el RTN '{dto.Rtn}'.");
            }

            var cliente = new Cliente
            {
                Rtn = rtnLimpio,
                NombreRazonSocial = dto.NombreRazonSocial.Trim(),
                NombreComercial = dto.NombreComercial?.Trim(),
                TipoPersona = dto.TipoPersona,
                Rubro = dto.Rubro?.Trim(),
                EmailPrincipal = dto.EmailPrincipal?.Trim(),
                EmailSecundario = dto.EmailSecundario?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                TelefonoWhatsApp = dto.TelefonoWhatsApp?.Trim(),
                Direccion = dto.Direccion?.Trim(),
                CuotaMensual = dto.CuotaMensual,
                DiaCobro = dto.DiaCobro,
                ContrasenaSAR = dto.ContrasenaSAR?.Trim(),
                Notas = dto.Notas?.Trim(),
                Activo = true
            };

            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            // Crear automáticamente el periodo fiscal del mes actual para el cliente
            var ahora = DateTime.UtcNow;
            var periodoMes = new PeriodoFiscalSAR
            {
                ClienteId = cliente.Id,
                Mes = ahora.Month,
                Anio = ahora.Year,
                FacturasRecibidas = false,
                LiquidadoSAR = false,
                Estado = "Pendiente"
            };

            await _context.PeriodosFiscalesSAR.AddAsync(periodoMes);
            await _context.SaveChangesAsync();

            return await GetClienteByIdAsync(cliente.Id);
        }

        public async Task<ClienteResponseDto> ActualizarClienteAsync(int id, ClienteUpdateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Rtn))
            {
                var rtnLimpio = dto.Rtn.Trim().Replace("-", "").Replace(" ", "");
                if (rtnLimpio != cliente.Rtn)
                {
                    if (await _context.Clientes.AnyAsync(c => c.Rtn == rtnLimpio && c.Id != id))
                    {
                        throw new InvalidOperationException($"Ya existe otro cliente con el RTN '{dto.Rtn}'.");
                    }
                    cliente.Rtn = rtnLimpio;
                }
            }

            cliente.NombreRazonSocial = dto.NombreRazonSocial.Trim();
            cliente.NombreComercial = dto.NombreComercial?.Trim();
            cliente.TipoPersona = dto.TipoPersona;
            cliente.Rubro = dto.Rubro?.Trim();
            cliente.EmailPrincipal = dto.EmailPrincipal?.Trim();
            cliente.EmailSecundario = dto.EmailSecundario?.Trim();
            cliente.Telefono = dto.Telefono?.Trim();
            cliente.TelefonoWhatsApp = dto.TelefonoWhatsApp?.Trim();
            cliente.Direccion = dto.Direccion?.Trim();
            cliente.CuotaMensual = dto.CuotaMensual;
            cliente.DiaCobro = dto.DiaCobro;
            cliente.ContrasenaSAR = dto.ContrasenaSAR?.Trim();
            cliente.Activo = dto.Activo;
            cliente.Notas = dto.Notas?.Trim();

            await _context.SaveChangesAsync();
            return await GetClienteByIdAsync(cliente.Id);
        }

        public async Task ToggleStatusAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            cliente.Activo = !cliente.Activo;
            await _context.SaveChangesAsync();
        }

        public async Task<ExpedienteFiscalDto> GetExpedienteFiscalAsync(int id)
        {
            var clienteDto = await GetClienteByIdAsync(id);

            // 1. Declaraciones Anuales
            var seguimientos = await _context.SeguimientosFiscalesAnuales
                .Where(s => s.ClienteId == id)
                .OrderByDescending(s => s.Anio)
                .ThenBy(s => s.TipoObligacion)
                .ToListAsync();

            var declaracionesAnuales = seguimientos.Select(s => new DeclaracionAnualItemDto
            {
                Id = s.Id,
                Anio = s.Anio,
                TipoObligacion = s.TipoObligacion,
                Titulo = s.TipoObligacion switch
                {
                    "ISR_ANUAL" => "Declaración y Pago Anual de ISR",
                    "PAGO_CUENTA_1" => "1er Pago a Cuenta (ISR)",
                    "PAGO_CUENTA_2" => "2do Pago a Cuenta (ISR)",
                    "PAGO_CUENTA_3" => "3er Pago a Cuenta (ISR)",
                    "RETENCIONES_ANUAL" => "Declaración Informativa Anual de Retenciones",
                    _ => s.TipoObligacion
                },
                FormularioSAR = s.TipoObligacion switch
                {
                    "ISR_ANUAL" => "SAR-352 / 353",
                    "PAGO_CUENTA_1" or "PAGO_CUENTA_2" or "PAGO_CUENTA_3" => "SAR-252",
                    "RETENCIONES_ANUAL" => "SAR-410",
                    _ => "SAR"
                },
                Estado = s.Estado,
                MontoDeclarado = s.MontoDeclarado,
                NumeroDeclaracionSAR = s.NumeroDeclaracionSAR,
                FechaCumplimiento = s.FechaCumplimiento,
                Observaciones = s.Observaciones
            }).ToList();

            // 2. Declaraciones Mensuales ISV (Día 10)
            var periodosSAR = await _context.PeriodosFiscalesSAR
                .Where(pf => pf.ClienteId == id)
                .OrderByDescending(pf => pf.Anio)
                .ThenByDescending(pf => pf.Mes)
                .ToListAsync();

            var meses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            var declaracionesMensuales = periodosSAR.Select(p => new PeriodoMensualItemDto
            {
                Id = p.Id,
                Mes = p.Mes,
                Anio = p.Anio,
                MesNombre = p.Mes >= 1 && p.Mes <= 12 ? meses[p.Mes] : p.Mes.ToString(),
                FacturasRecibidas = p.FacturasRecibidas,
                FechaRecepcionFacturas = p.FechaRecepcionFacturas,
                CantidadFacturasVenta = p.CantidadFacturasVenta,
                CantidadFacturasCompra = p.CantidadFacturasCompra,
                LiquidadoSAR = p.LiquidadoSAR,
                FechaLiquidacion = p.FechaLiquidacion,
                NumeroDeclaracionSAR = p.NumeroDeclaracionSAR,
                MontoImpuestoISV = p.MontoImpuestoISV,
                Estado = p.Estado
            }).ToList();

            // 3. Comprobantes / Recibos y Facturas
            var recibos = await _context.Recibos
                .Include(r => r.PagoHonorario)
                .Where(r => r.PagoHonorario.ClienteId == id)
                .OrderByDescending(r => r.FechaEmision)
                .ToListAsync();

            var comprobantes = recibos.Select(r => new ComprobanteItemDto
            {
                ReciboId = r.Id,
                NumeroRecibo = r.NumeroRecibo,
                NumeroFiscal = r.NumeroFiscal,
                Cai = r.Cai,
                Monto = r.Monto,
                MontoEnLetras = r.MontoEnLetras,
                FechaEmision = r.FechaEmision,
                Concepto = r.Concepto,
                MetodoPago = r.PagoHonorario.MetodoPago,
                MesAplicado = r.PagoHonorario.MesAplicado
            }).ToList();

            var totalDeclaraciones = declaracionesAnuales.Count(d => d.Estado == "Declarado") + declaracionesMensuales.Count(m => m.LiquidadoSAR);
            var totalImpuestoSAR = declaracionesAnuales.Sum(d => d.MontoDeclarado ?? 0) + declaracionesMensuales.Sum(m => m.MontoImpuestoISV ?? 0);
            var totalHonorarios = comprobantes.Sum(c => c.Monto);

            return new ExpedienteFiscalDto
            {
                Cliente = clienteDto,
                DeclaracionesAnuales = declaracionesAnuales,
                DeclaracionesMensualesISV = declaracionesMensuales,
                ComprobantesEmitidos = comprobantes,
                TotalDeclaracionesPresentadas = totalDeclaraciones,
                TotalImpuestoLiquidadoSAR = totalImpuestoSAR,
                TotalHonorariosPagados = totalHonorarios
            };
        }

        // ==========================================
        // IMPORTACIÓN MASIVA DE CLIENTES (EXCEL/CSV)
        // ==========================================

        public async Task<ClienteImportResponseDto> ImportarClientesMasivoAsync(List<ClienteImportItemDto> items)
        {
            var response = new ClienteImportResponseDto
            {
                TotalProcesados = items.Count
            };

            if (items == null || items.Count == 0)
            {
                response.Mensajes.Add("No se enviaron registros para procesar.");
                return response;
            }

            var clientesExistentes = await _context.Clientes.ToListAsync();
            var clientesDict = clientesExistentes.ToDictionary(c => c.Rtn.Trim().Replace("-", "").ToUpper(), c => c);

            var rubrosExistentes = await _context.Rubros.ToListAsync();
            var rubrosDict = rubrosExistentes.ToDictionary(r => r.Nombre.Trim().ToUpper(), r => r);

            int seq = 1;
            foreach (var item in items)
            {
                var cleanRtn = (item.Rtn ?? string.Empty).Trim().Replace("-", "").Replace(" ", "").ToUpper();
                var razonSocial = (item.NombreRazonSocial ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cleanRtn))
                {
                    response.TotalErrores++;
                    response.Mensajes.Add($"Fila #{seq}: El RTN está vacío.");
                    seq++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(razonSocial))
                {
                    response.TotalErrores++;
                    response.Mensajes.Add($"Fila #{seq} (RTN {cleanRtn}): La Razón Social o Nombre es obligatorio.");
                    seq++;
                    continue;
                }

                // Asegurar Rubro
                var rubroNombre = string.IsNullOrWhiteSpace(item.Rubro) ? "Comercio General" : item.Rubro.Trim();
                if (!rubrosDict.TryGetValue(rubroNombre.ToUpper(), out var rubroObj))
                {
                    rubroObj = new Rubro
                    {
                        Nombre = rubroNombre,
                        Descripcion = $"Rubro creado automáticamente ({rubroNombre})",
                        Activo = true
                    };
                    _context.Rubros.Add(rubroObj);
                    await _context.SaveChangesAsync();
                    rubrosDict[rubroNombre.ToUpper()] = rubroObj;
                }

                var tipoPersona = (item.TipoPersona ?? string.Empty).Trim().ToLower() == "natural" ? "Natural" : "Juridica";
                var diaCobro = item.DiaCobro >= 1 && item.DiaCobro <= 31 ? item.DiaCobro : 5;
                var cuota = item.CuotaMensual >= 0 ? item.CuotaMensual : 0.00m;

                if (clientesDict.TryGetValue(cleanRtn, out var clienteExistente))
                {
                    // Actualizar cliente existente
                    clienteExistente.NombreRazonSocial = razonSocial;
                    if (!string.IsNullOrWhiteSpace(item.NombreComercial)) clienteExistente.NombreComercial = item.NombreComercial.Trim();
                    clienteExistente.TipoPersona = tipoPersona;
                    clienteExistente.Rubro = rubroNombre;
                    if (!string.IsNullOrWhiteSpace(item.ContrasenaSAR)) clienteExistente.ContrasenaSAR = item.ContrasenaSAR.Trim();
                    if (!string.IsNullOrWhiteSpace(item.EmailPrincipal)) clienteExistente.EmailPrincipal = item.EmailPrincipal.Trim();
                    if (!string.IsNullOrWhiteSpace(item.EmailSecundario)) clienteExistente.EmailSecundario = item.EmailSecundario.Trim();
                    if (!string.IsNullOrWhiteSpace(item.Telefono)) clienteExistente.Telefono = item.Telefono.Trim();
                    if (!string.IsNullOrWhiteSpace(item.TelefonoWhatsApp)) clienteExistente.TelefonoWhatsApp = item.TelefonoWhatsApp.Trim();
                    if (!string.IsNullOrWhiteSpace(item.Direccion)) clienteExistente.Direccion = item.Direccion.Trim();
                    if (cuota > 0) clienteExistente.CuotaMensual = cuota;
                    clienteExistente.DiaCobro = diaCobro;
                    if (!string.IsNullOrWhiteSpace(item.Notas)) clienteExistente.Notas = item.Notas.Trim();
                    clienteExistente.Activo = true;

                    response.TotalActualizados++;
                }
                else
                {
                    // Crear nuevo cliente
                    var nuevoCliente = new Cliente
                    {
                        Rtn = cleanRtn,
                        NombreRazonSocial = razonSocial,
                        NombreComercial = item.NombreComercial?.Trim(),
                        TipoPersona = tipoPersona,
                        Rubro = rubroNombre,
                        ContrasenaSAR = item.ContrasenaSAR?.Trim(),
                        EmailPrincipal = item.EmailPrincipal?.Trim(),
                        EmailSecundario = item.EmailSecundario?.Trim(),
                        Telefono = item.Telefono?.Trim(),
                        TelefonoWhatsApp = item.TelefonoWhatsApp?.Trim(),
                        Direccion = item.Direccion?.Trim(),
                        CuotaMensual = cuota,
                        DiaCobro = diaCobro,
                        Notas = item.Notas?.Trim(),
                        Activo = true
                    };

                    _context.Clientes.Add(nuevoCliente);
                    clientesDict[cleanRtn] = nuevoCliente;
                    response.TotalGuardados++;
                }

                seq++;
            }

            await _context.SaveChangesAsync();
            return response;
        }

        public byte[] GenerarPlantillaClientesCsv()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("RTN,NombreRazonSocial,NombreComercial,TipoPersona,Rubro,ContrasenaSAR,EmailPrincipal,EmailSecundario,Telefono,TelefonoWhatsApp,Direccion,CuotaMensual,DiaCobro,Notas");
            csv.AppendLine("08011990123456,DISTRIBUIDORA EJEMPLO S.A.,Comercial Ejemplo,Juridica,Comercio General,SarPass2026*,contacto@ejemplo.hn,,+504 2235-0000,+504 9988-7766,Tegucigalpa M.D.C.,2500.00,5,Cliente Activo");
            csv.AppendLine("08011985654321,JUAN CARLOS PEREZ LOPEZ,Taller Perez,Natural,Servicios,Perez2026*,juan@taller.hn,,+504 2550-1122,+504 8877-6655,San Pedro Sula,1800.00,10,Declara ISV mensual");

            return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        }
    }
}
