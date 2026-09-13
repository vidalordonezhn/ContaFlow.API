using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.CAI
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CAIController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public CAIController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet("activo")]
        public async Task<ActionResult<CAIResponseDto>> GetCAIActivo()
        {
            var cai = await _context.AutorizacionesCAI
                .Where(c => c.Activo)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (cai == null)
            {
                return Ok(null);
            }

            return Ok(MapToDto(cai));
        }

        [HttpGet]
        public async Task<ActionResult<List<CAIResponseDto>>> GetHistorialCAI()
        {
            var lista = await _context.AutorizacionesCAI
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return Ok(lista.Select(MapToDto).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<CAIResponseDto>> RegistrarNuevoCAI([FromBody] CAICreateDto dto)
        {
            // Si el nuevo se marca como activo, desactivamos los anteriores
            if (dto.Activo)
            {
                var anteriores = await _context.AutorizacionesCAI.Where(c => c.Activo).ToListAsync();
                foreach (var a in anteriores)
                {
                    a.Activo = false;
                }
            }

            var cai = new AutorizacionCAI
            {
                Cai = dto.Cai.Trim().ToUpper(),
                TipoDocumento = dto.TipoDocumento.Trim(),
                Establecimiento = dto.Establecimiento.Trim().PadLeft(3, '0'),
                PuntoEmision = dto.PuntoEmision.Trim().PadLeft(3, '0'),
                TipoDocCodigo = dto.TipoDocCodigo.Trim().PadLeft(2, '0'),
                RangoInicial = dto.RangoInicial,
                RangoFinal = dto.RangoFinal,
                CorrelativoActual = dto.RangoInicial,
                FechaLimiteEmision = dto.FechaLimiteEmision.ToUniversalTime(),
                FechaRecepcionSAR = dto.FechaRecepcionSAR?.ToUniversalTime(),
                Activo = dto.Activo,
                Observaciones = dto.Observaciones?.Trim()
            };

            await _context.AutorizacionesCAI.AddAsync(cai);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCAIActivo), MapToDto(cai));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CAIResponseDto>> ActualizarCAI(int id, [FromBody] CAIUpdateDto dto)
        {
            var cai = await _context.AutorizacionesCAI.FindAsync(id);
            if (cai == null)
            {
                return NotFound(new { mensaje = "Autorización CAI no encontrada." });
            }

            cai.Cai = dto.Cai.Trim().ToUpper();
            cai.TipoDocumento = dto.TipoDocumento.Trim();
            cai.Establecimiento = dto.Establecimiento.Trim().PadLeft(3, '0');
            cai.PuntoEmision = dto.PuntoEmision.Trim().PadLeft(3, '0');
            cai.TipoDocCodigo = dto.TipoDocCodigo.Trim().PadLeft(2, '0');
            cai.RangoInicial = dto.RangoInicial;
            cai.RangoFinal = dto.RangoFinal;
            cai.FechaLimiteEmision = dto.FechaLimiteEmision.ToUniversalTime();
            cai.Activo = dto.Activo;
            cai.Observaciones = dto.Observaciones?.Trim();

            if (dto.Activo)
            {
                var otros = await _context.AutorizacionesCAI.Where(c => c.Id != id && c.Activo).ToListAsync();
                foreach (var o in otros) o.Activo = false;
            }

            await _context.SaveChangesAsync();
            return Ok(MapToDto(cai));
        }

        private static CAIResponseDto MapToDto(AutorizacionCAI c)
        {
            return new CAIResponseDto
            {
                Id = c.Id,
                Cai = c.Cai,
                TipoDocumento = c.TipoDocumento,
                Establecimiento = c.Establecimiento,
                PuntoEmision = c.PuntoEmision,
                TipoDocCodigo = c.TipoDocCodigo,
                RangoInicial = c.RangoInicial,
                RangoFinal = c.RangoFinal,
                CorrelativoActual = c.CorrelativoActual,
                RangoInicialFormateado = c.RangoInicialFormateado,
                RangoFinalFormateado = c.RangoFinalFormateado,
                SiguienteNumeroFormateado = c.SiguienteNumeroFormateado,
                FechaLimiteEmision = c.FechaLimiteEmision,
                FechaRecepcionSAR = c.FechaRecepcionSAR,
                Activo = c.Activo,
                Observaciones = c.Observaciones,
                TotalAutorizados = c.TotalAutorizados,
                TotalConsumidos = c.TotalConsumidos,
                TotalRestantes = c.TotalRestantes,
                PorcentajeConsumido = c.PorcentajeConsumido,
                EstaAgotado = c.EstaAgotado,
                EstaVencido = c.EstaVencido,
                DiasRestantes = c.DiasRestantes
            };
        }
    }

    public class CAIResponseDto
    {
        public int Id { get; set; }
        public string Cai { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string Establecimiento { get; set; } = string.Empty;
        public string PuntoEmision { get; set; } = string.Empty;
        public string TipoDocCodigo { get; set; } = string.Empty;
        public int RangoInicial { get; set; }
        public int RangoFinal { get; set; }
        public int CorrelativoActual { get; set; }
        public string RangoInicialFormateado { get; set; } = string.Empty;
        public string RangoFinalFormateado { get; set; } = string.Empty;
        public string SiguienteNumeroFormateado { get; set; } = string.Empty;
        public DateTime FechaLimiteEmision { get; set; }
        public DateTime? FechaRecepcionSAR { get; set; }
        public bool Activo { get; set; }
        public string? Observaciones { get; set; }
        public int TotalAutorizados { get; set; }
        public int TotalConsumidos { get; set; }
        public int TotalRestantes { get; set; }
        public double PorcentajeConsumido { get; set; }
        public bool EstaAgotado { get; set; }
        public bool EstaVencido { get; set; }
        public int DiasRestantes { get; set; }
    }

    public class CAICreateDto
    {
        public string Cai { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = "Recibo por Honorarios Profesionales";
        public string Establecimiento { get; set; } = "000";
        public string PuntoEmision { get; set; } = "001";
        public string TipoDocCodigo { get; set; } = "01";
        public int RangoInicial { get; set; } = 1;
        public int RangoFinal { get; set; } = 500;
        public DateTime FechaLimiteEmision { get; set; }
        public DateTime? FechaRecepcionSAR { get; set; }
        public bool Activo { get; set; } = true;
        public string? Observaciones { get; set; }
    }

    public class CAIUpdateDto
    {
        public string Cai { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = "Recibo por Honorarios Profesionales";
        public string Establecimiento { get; set; } = "000";
        public string PuntoEmision { get; set; } = "001";
        public string TipoDocCodigo { get; set; } = "01";
        public int RangoInicial { get; set; }
        public int RangoFinal { get; set; }
        public DateTime FechaLimiteEmision { get; set; }
        public bool Activo { get; set; }
        public string? Observaciones { get; set; }
    }
}
