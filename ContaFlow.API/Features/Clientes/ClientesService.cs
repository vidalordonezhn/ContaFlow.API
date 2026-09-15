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
                    Dni = c.Dni,
                    RepresentanteLegalNombre = c.RepresentanteLegalNombre,
                    RepresentanteLegalRtn = c.RepresentanteLegalRtn,
                    DepartamentoId = c.DepartamentoId,
                    DepartamentoNombre = c.DepartamentoNombre,
                    MunicipioId = c.MunicipioId,
                    MunicipioNombre = c.MunicipioNombre,
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
                Dni = c.Dni,
                RepresentanteLegalNombre = c.RepresentanteLegalNombre,
                RepresentanteLegalRtn = c.RepresentanteLegalRtn,
                DepartamentoId = c.DepartamentoId,
                DepartamentoNombre = c.DepartamentoNombre,
                MunicipioId = c.MunicipioId,
                MunicipioNombre = c.MunicipioNombre,
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
                Dni = dto.Dni?.Trim(),
                RepresentanteLegalNombre = dto.RepresentanteLegalNombre?.Trim(),
                RepresentanteLegalRtn = dto.RepresentanteLegalRtn?.Trim(),
                DepartamentoId = dto.DepartamentoId,
                DepartamentoNombre = dto.DepartamentoNombre?.Trim(),
                MunicipioId = dto.MunicipioId,
                MunicipioNombre = dto.MunicipioNombre?.Trim(),
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
            cliente.Dni = dto.Dni?.Trim();
            cliente.RepresentanteLegalNombre = dto.RepresentanteLegalNombre?.Trim();
            cliente.RepresentanteLegalRtn = dto.RepresentanteLegalRtn?.Trim();
            cliente.DepartamentoId = dto.DepartamentoId;
            cliente.DepartamentoNombre = dto.DepartamentoNombre?.Trim();
            cliente.MunicipioId = dto.MunicipioId;
            cliente.MunicipioNombre = dto.MunicipioNombre?.Trim();
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
                .Where(r => r.ClienteId == id || (r.PagoHonorario != null && r.PagoHonorario.ClienteId == id))
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
                MetodoPago = r.MetodoPago ?? r.PagoHonorario?.MetodoPago ?? "Transferencia",
                MesAplicado = r.PagoHonorario?.MesAplicado ?? (!string.IsNullOrWhiteSpace(r.Concepto) ? r.Concepto : $"{r.FechaEmision:MMMM yyyy}")
            }).ToList();

            var ahora = DateTime.UtcNow;
            var mesActual = ahora.Month;
            var anioActual = ahora.Year;

            var totalDeclaraciones = declaracionesAnuales.Count(d => d.Estado == "Declarado") + declaracionesMensuales.Count(m => m.LiquidadoSAR);
            var totalImpuestoSAR = declaracionesAnuales.Sum(d => d.MontoDeclarado ?? 0) + declaracionesMensuales.Sum(m => m.MontoImpuestoISV ?? 0);
            var totalHonorarios = comprobantes.Sum(c => c.Monto);

            // 1. EVALUACIÓN FINANCIERA Y COBRANZA
            decimal cuota = clienteDto.CuotaMensual;
            decimal totalEsperado = 0m;
            decimal saldoHonorarios = 0m;
            string estadoCobranza = "AlDia";
            string mensajeCobranza = string.Empty;
            int mesesDeuda = 0;

            if (cuota <= 0)
            {
                estadoCobranza = "PorGestion";
                mensajeCobranza = "Servicios Contables por Evento / Gestión Específica";
                totalEsperado = 0;
                saldoHonorarios = totalHonorarios;
            }
            else
            {
                totalEsperado = cuota * mesActual;
                saldoHonorarios = totalHonorarios - totalEsperado;

                if (saldoHonorarios > 0)
                {
                    estadoCobranza = "SaldoAFavor";
                    int mesesCubiertos = (int)(totalHonorarios / cuota);
                    if (mesesCubiertos >= 12)
                    {
                        mensajeCobranza = $"Cubierto todo el año {anioActual} (Saldo a favor: L. {saldoHonorarios:N2})";
                    }
                    else
                    {
                        var mesCubiertoNombre = mesesCubiertos >= 1 && mesesCubiertos <= 12 ? meses[mesesCubiertos] : $"{mesesCubiertos} meses";
                        mensajeCobranza = $"Cubierto hasta {mesCubiertoNombre} {anioActual} (Saldo a favor: L. {saldoHonorarios:N2})";
                    }
                }
                else if (saldoHonorarios == 0)
                {
                    estadoCobranza = "AlDia";
                    mensajeCobranza = $"Al día con sus cuotas hasta {meses[mesActual]} {anioActual}";
                }
                else
                {
                    estadoCobranza = "Pendiente";
                    decimal deuda = Math.Abs(saldoHonorarios);
                    mesesDeuda = (int)Math.Ceiling(deuda / cuota);
                    mensajeCobranza = $"Pendiente de pago: L. {deuda:N2} ({mesesDeuda} {(mesesDeuda == 1 ? "mes" : "meses")} de retraso)";
                }
            }

            // 2. SEMÁFORO ISV MENSUAL (DÍA 10)
            string semaforoIsv = "AlDia";
            string detalleIsv = string.Empty;

            var periodoActual = declaracionesMensuales.FirstOrDefault(m => m.Anio == anioActual && m.Mes == mesActual);
            var periodoAnterior = declaracionesMensuales.FirstOrDefault(m => m.Anio == anioActual && m.Mes == (mesActual > 1 ? mesActual - 1 : 12));

            if (periodoActual != null && periodoActual.LiquidadoSAR)
            {
                semaforoIsv = "AlDia";
                detalleIsv = $"Declaración de {meses[mesActual]} liquidada ante el SAR";
            }
            else if (periodoAnterior != null && !periodoAnterior.LiquidadoSAR)
            {
                semaforoIsv = "Pendiente";
                detalleIsv = $"Declaración de {periodoAnterior.MesNombre} pendiente de presentar al SAR (Venció día 10)";
            }
            else if (periodoActual != null && periodoActual.FacturasRecibidas)
            {
                semaforoIsv = "EnProceso";
                detalleIsv = $"Facturas de {meses[mesActual]} recibidas, listas para cálculo de ISV";
            }
            else
            {
                semaforoIsv = ahora.Day > 10 ? "Pendiente" : "EnProceso";
                detalleIsv = $"Período {meses[mesActual]}: Esperando facturas para declaración ISV";
            }

            // 3. SEMÁFORO PAGOS A CUENTA & ANUAL
            string semaforoPagos = "AlDia";
            string detallePagos = string.Empty;

            var p1 = declaracionesAnuales.FirstOrDefault(d => d.Anio == anioActual && d.TipoObligacion == "PAGO_CUENTA_1");
            var p2 = declaracionesAnuales.FirstOrDefault(d => d.Anio == anioActual && d.TipoObligacion == "PAGO_CUENTA_2");
            var p3 = declaracionesAnuales.FirstOrDefault(d => d.Anio == anioActual && d.TipoObligacion == "PAGO_CUENTA_3");
            var isr = declaracionesAnuales.FirstOrDefault(d => d.Anio == anioActual && d.TipoObligacion == "ISR_ANUAL");

            if (mesActual <= 4)
            {
                if (isr != null && isr.Estado == "Declarado")
                {
                    semaforoPagos = "AlDia";
                    detallePagos = $"ISR Anual {anioActual - 1} presentado exitosamente";
                }
                else
                {
                    semaforoPagos = "ProximoVencimiento";
                    detallePagos = $"Declaración Anual ISR vence el 30 de Abril";
                }
            }
            else if (mesActual <= 6)
            {
                if (p1 != null && p1.Estado == "Declarado")
                {
                    semaforoPagos = "AlDia";
                    detallePagos = "1er Pago a Cuenta (30 Jun) cancelado";
                }
                else
                {
                    semaforoPagos = "ProximoVencimiento";
                    detallePagos = "1er Pago a Cuenta vence el 30 de Junio";
                }
            }
            else if (mesActual <= 9)
            {
                if (p2 != null && p2.Estado == "Declarado")
                {
                    semaforoPagos = "AlDia";
                    detallePagos = "2do Pago a Cuenta (30 Sep) cancelado";
                }
                else
                {
                    semaforoPagos = "ProximoVencimiento";
                    detallePagos = "2do Pago a Cuenta vence el 30 de Septiembre";
                }
            }
            else
            {
                if (p3 != null && p3.Estado == "Declarado")
                {
                    semaforoPagos = "AlDia";
                    detallePagos = "3er Pago a Cuenta (31 Dic) cancelado";
                }
                else
                {
                    semaforoPagos = "ProximoVencimiento";
                    detallePagos = "3er Pago a Cuenta vence el 31 de Diciembre";
                }
            }

            // 4. SEMÁFORO CAI / FACTURACIÓN PROPIA
            string semaforoCai = "SinCAI";
            string detalleCai = "Sin CAI registrado en el despacho";
            int? diasVencimientoCai = null;

            var caiActivo = await _context.AutorizacionesCAI
                .Where(c => c.Activo)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (caiActivo != null)
            {
                diasVencimientoCai = caiActivo.DiasRestantes;
                if (caiActivo.EstaVencido)
                {
                    semaforoCai = "Vencido";
                    detalleCai = $"CAI Vencido el {caiActivo.FechaLimiteEmision:dd/MM/yyyy}";
                }
                else if (caiActivo.DiasRestantes <= 30)
                {
                    semaforoCai = "ProximoAVencer";
                    detalleCai = $"Vence en {caiActivo.DiasRestantes} días ({caiActivo.FechaLimiteEmision:dd/MM/yyyy})";
                }
                else
                {
                    semaforoCai = "Vigente";
                    detalleCai = $"Vigente hasta {caiActivo.FechaLimiteEmision:dd/MM/yyyy} ({caiActivo.DiasRestantes} días restantes)";
                }
            }

            return new ExpedienteFiscalDto
            {
                Cliente = clienteDto,
                DeclaracionesAnuales = declaracionesAnuales,
                DeclaracionesMensualesISV = declaracionesMensuales,
                ComprobantesEmitidos = comprobantes,
                TotalDeclaracionesPresentadas = totalDeclaraciones,
                TotalImpuestoLiquidadoSAR = totalImpuestoSAR,
                TotalHonorariosPagados = totalHonorarios,
                TotalHonorariosEsperados = totalEsperado,
                SaldoHonorarios = saldoHonorarios,
                EstadoCobranza = estadoCobranza,
                MensajeCobranza = mensajeCobranza,
                MesesDeuda = mesesDeuda,
                SemaforoISV = semaforoIsv,
                DetalleISV = detalleIsv,
                SemaforoPagosACuenta = semaforoPagos,
                DetallePagosACuenta = detallePagos,
                SemaforoCAI = semaforoCai,
                DetalleCAI = detalleCai,
                DiasVencimientoCAI = diasVencimientoCai
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
                    if (!string.IsNullOrWhiteSpace(item.Dni)) clienteExistente.Dni = item.Dni.Trim();
                    if (!string.IsNullOrWhiteSpace(item.RepresentanteLegalNombre)) clienteExistente.RepresentanteLegalNombre = item.RepresentanteLegalNombre.Trim();
                    if (!string.IsNullOrWhiteSpace(item.RepresentanteLegalRtn)) clienteExistente.RepresentanteLegalRtn = item.RepresentanteLegalRtn.Trim();
                    if (!string.IsNullOrWhiteSpace(item.DepartamentoNombre)) clienteExistente.DepartamentoNombre = item.DepartamentoNombre.Trim();
                    if (!string.IsNullOrWhiteSpace(item.MunicipioNombre)) clienteExistente.MunicipioNombre = item.MunicipioNombre.Trim();
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
                        Dni = item.Dni?.Trim(),
                        RepresentanteLegalNombre = item.RepresentanteLegalNombre?.Trim(),
                        RepresentanteLegalRtn = item.RepresentanteLegalRtn?.Trim(),
                        DepartamentoNombre = item.DepartamentoNombre?.Trim(),
                        MunicipioNombre = item.MunicipioNombre?.Trim(),
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
