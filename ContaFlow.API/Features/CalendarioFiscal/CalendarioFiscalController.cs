using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.CalendarioFiscal
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendarioFiscalController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public CalendarioFiscalController(ContaFlowDbContext context)
        {
            _context = context;
        }

        // Definición estática de las obligaciones del calendario tributario hondureño
        private static readonly List<ObligacionInfo> CatalogoObligaciones = new()
        {
            new ObligacionInfo
            {
                Codigo = "RETENCIONES_ANUAL",
                Titulo = "Declaración Anual de Retenciones",
                FormularioSAR = "SAR-410",
                Descripcion = "Declaración jurada anual informativa de retenciones en la fuente efectuadas durante el ejercicio fiscal anterior.",
                DiaVencimiento = 31,
                MesVencimiento = 1,
                MesNombre = "Enero",
                TipoPeriodicidad = "Anual",
                Color = "#3b82f6" // Azul
            },
            new ObligacionInfo
            {
                Codigo = "ISR_ANUAL",
                Titulo = "Declaración y Pago Anual de ISR",
                FormularioSAR = "SAR-352 / 353",
                Descripcion = "Liquidación y pago anual del Impuesto Sobre la Renta, Aportación Solidaria y Activo Neto de personas naturales y jurídicas.",
                DiaVencimiento = 30,
                MesVencimiento = 4,
                MesNombre = "Abril",
                TipoPeriodicidad = "Anual",
                Color = "#ef4444" // Rojo crítico
            },
            new ObligacionInfo
            {
                Codigo = "PAGO_CUENTA_1",
                Titulo = "1er Pago a Cuenta (ISR)",
                FormularioSAR = "SAR-252",
                Descripcion = "Primera cuota obligatoria de pagos a cuenta del Impuesto Sobre la Renta (ejercicio fiscal en curso).",
                DiaVencimiento = 30,
                MesVencimiento = 6,
                MesNombre = "Junio",
                TipoPeriodicidad = "Trimestral",
                Color = "#f59e0b" // Ámbar
            },
            new ObligacionInfo
            {
                Codigo = "PAGO_CUENTA_2",
                Titulo = "2do Pago a Cuenta (ISR)",
                FormularioSAR = "SAR-252",
                Descripcion = "Segunda cuota obligatoria de pagos a cuenta del Impuesto Sobre la Renta.",
                DiaVencimiento = 30,
                MesVencimiento = 9,
                MesNombre = "Septiembre",
                TipoPeriodicidad = "Trimestral",
                Color = "#f59e0b" // Ámbar
            },
            new ObligacionInfo
            {
                Codigo = "PAGO_CUENTA_3",
                Titulo = "3er Pago a Cuenta (ISR)",
                FormularioSAR = "SAR-252",
                Descripcion = "Tercera y última cuota de pagos a cuenta del Impuesto Sobre la Renta.",
                DiaVencimiento = 31,
                MesVencimiento = 12,
                MesNombre = "Diciembre",
                TipoPeriodicidad = "Trimestral",
                Color = "#f59e0b" // Ámbar
            }
        };

        [HttpGet("{anio:int}")]
        public async Task<ActionResult<CalendarioFiscalResumenDto>> GetCalendarioAnual(int anio)
        {
            var hoy = DateTime.UtcNow;
            var clientes = await _context.Clientes.Where(c => c.Activo).ToListAsync();
            var seguimientos = await _context.SeguimientosFiscalesAnuales
                .Where(s => s.Anio == anio)
                .ToListAsync();

            var hitos = new List<HitoFiscalDto>();

            foreach (var ob in CatalogoObligaciones)
            {
                var fechaVencimiento = new DateTime(anio, ob.MesVencimiento, ob.DiaVencimiento, 23, 59, 59, DateTimeKind.Utc);
                var diasRestantes = (int)Math.Ceiling((fechaVencimiento - hoy).TotalDays);
                var estaVencido = hoy > fechaVencimiento;

                // Contar cumplimientos
                var segsObligacion = seguimientos.Where(s => s.TipoObligacion == ob.Codigo).ToList();
                var declaradosCount = segsObligacion.Count(s => s.Estado == "Declarado");
                var enProcesoCount = segsObligacion.Count(s => s.Estado == "EnProceso");
                var noAplicaCount = segsObligacion.Count(s => s.Estado == "NoAplica");
                var totalAplicables = Math.Max(0, clientes.Count - noAplicaCount);
                var pendientesCount = Math.Max(0, totalAplicables - declaradosCount - enProcesoCount);
                var porcentajeCumplimiento = totalAplicables > 0 ? Math.Round((double)declaradosCount / totalAplicables * 100, 1) : 0;

                hitos.Add(new HitoFiscalDto
                {
                    Codigo = ob.Codigo,
                    Titulo = ob.Titulo,
                    FormularioSAR = ob.FormularioSAR,
                    Descripcion = ob.Descripcion,
                    FechaVencimiento = fechaVencimiento,
                    DiaVencimiento = ob.DiaVencimiento,
                    MesVencimiento = ob.MesVencimiento,
                    MesNombre = ob.MesNombre,
                    TipoPeriodicidad = ob.TipoPeriodicidad,
                    Color = ob.Color,
                    DiasRestantes = diasRestantes,
                    EstaVencido = estaVencido,
                    TotalClientes = clientes.Count,
                    DeclaradosCount = declaradosCount,
                    EnProcesoCount = enProcesoCount,
                    PendientesCount = pendientesCount,
                    NoAplicaCount = noAplicaCount,
                    PorcentajeCumplimiento = porcentajeCumplimiento
                });
            }

            // Hito más próximo en el futuro
            var proximoHito = hitos
                .Where(h => !h.EstaVencido)
                .OrderBy(h => h.FechaVencimiento)
                .FirstOrDefault() ?? hitos.OrderByDescending(h => h.FechaVencimiento).FirstOrDefault();

            return Ok(new CalendarioFiscalResumenDto
            {
                Anio = anio,
                Hitos = hitos,
                ProximoHito = proximoHito
            });
        }

        [HttpGet("{anio:int}/obligacion/{codigo}")]
        public async Task<ActionResult<List<ClienteSeguimientoDto>>> GetClientesPorObligacion(int anio, string codigo)
        {
            var clientes = await _context.Clientes.Where(c => c.Activo).OrderBy(c => c.NombreRazonSocial).ToListAsync();
            var seguimientos = await _context.SeguimientosFiscalesAnuales
                .Where(s => s.Anio == anio && s.TipoObligacion == codigo)
                .ToDictionaryAsync(s => s.ClienteId);

            var resultado = new List<ClienteSeguimientoDto>();

            foreach (var c in clientes)
            {
                seguimientos.TryGetValue(c.Id, out var seg);

                resultado.Add(new ClienteSeguimientoDto
                {
                    ClienteId = c.Id,
                    NombreRazonSocial = c.NombreRazonSocial,
                    NombreComercial = c.NombreComercial,
                    Rtn = c.Rtn,
                    TipoPersona = c.TipoPersona,
                    Rubro = c.Rubro,
                    TelefonoWhatsApp = c.TelefonoWhatsApp ?? c.Telefono,
                    Email = c.EmailPrincipal,
                    Anio = anio,
                    TipoObligacion = codigo,
                    SeguimientoId = seg?.Id,
                    Estado = seg?.Estado ?? "Pendiente",
                    MontoDeclarado = seg?.MontoDeclarado,
                    NumeroDeclaracionSAR = seg?.NumeroDeclaracionSAR,
                    FechaCumplimiento = seg?.FechaCumplimiento,
                    Observaciones = seg?.Observaciones
                });
            }

            return Ok(resultado);
        }

        [HttpPut("actualizar-estado")]
        public async Task<ActionResult> ActualizarEstadoCliente([FromBody] ActualizarSeguimientoDto dto)
        {
            var seguimiento = await _context.SeguimientosFiscalesAnuales
                .FirstOrDefaultAsync(s => s.ClienteId == dto.ClienteId && s.Anio == dto.Anio && s.TipoObligacion == dto.TipoObligacion);

            if (seguimiento == null)
            {
                seguimiento = new SeguimientoFiscalAnual
                {
                    ClienteId = dto.ClienteId,
                    Anio = dto.Anio,
                    TipoObligacion = dto.TipoObligacion,
                    Estado = dto.Estado,
                    MontoDeclarado = dto.MontoDeclarado,
                    NumeroDeclaracionSAR = dto.NumeroDeclaracionSAR?.Trim(),
                    FechaCumplimiento = dto.Estado == "Declarado" ? (dto.FechaCumplimiento ?? DateTime.UtcNow) : null,
                    Observaciones = dto.Observaciones?.Trim()
                };
                await _context.SeguimientosFiscalesAnuales.AddAsync(seguimiento);
            }
            else
            {
                seguimiento.Estado = dto.Estado;
                seguimiento.MontoDeclarado = dto.MontoDeclarado;
                seguimiento.NumeroDeclaracionSAR = dto.NumeroDeclaracionSAR?.Trim();
                seguimiento.FechaCumplimiento = dto.Estado == "Declarado" ? (dto.FechaCumplimiento ?? DateTime.UtcNow) : null;
                seguimiento.Observaciones = dto.Observaciones?.Trim();
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Estado actualizado correctamente.", id = seguimiento.Id });
        }
    }

    public class ObligacionInfo
    {
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string FormularioSAR { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int DiaVencimiento { get; set; }
        public int MesVencimiento { get; set; }
        public string MesNombre { get; set; } = string.Empty;
        public string TipoPeriodicidad { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class HitoFiscalDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string FormularioSAR { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int DiaVencimiento { get; set; }
        public int MesVencimiento { get; set; }
        public string MesNombre { get; set; } = string.Empty;
        public string TipoPeriodicidad { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int DiasRestantes { get; set; }
        public bool EstaVencido { get; set; }
        public int TotalClientes { get; set; }
        public int DeclaradosCount { get; set; }
        public int EnProcesoCount { get; set; }
        public int PendientesCount { get; set; }
        public int NoAplicaCount { get; set; }
        public double PorcentajeCumplimiento { get; set; }
    }

    public class CalendarioFiscalResumenDto
    {
        public int Anio { get; set; }
        public List<HitoFiscalDto> Hitos { get; set; } = new();
        public HitoFiscalDto? ProximoHito { get; set; }
    }

    public class ClienteSeguimientoDto
    {
        public int ClienteId { get; set; }
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string Rtn { get; set; } = string.Empty;
        public string TipoPersona { get; set; } = string.Empty;
        public string? Rubro { get; set; }
        public string? TelefonoWhatsApp { get; set; }
        public string? Email { get; set; }
        public int Anio { get; set; }
        public string TipoObligacion { get; set; } = string.Empty;
        public int? SeguimientoId { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public decimal? MontoDeclarado { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public DateTime? FechaCumplimiento { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ActualizarSeguimientoDto
    {
        public int ClienteId { get; set; }
        public int Anio { get; set; }
        public string TipoObligacion { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
        public decimal? MontoDeclarado { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public DateTime? FechaCumplimiento { get; set; }
        public string? Observaciones { get; set; }
    }
}
