using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Recordatorios.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.Recordatorios
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecordatoriosController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public RecordatoriosController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<RecordatorioResponseDto>>> GetRecordatorios(
            [FromQuery] int? clienteId,
            [FromQuery] string? tipo,
            [FromQuery] string? estado)
        {
            var query = _context.RecordatoriosClientes
                .Include(r => r.Cliente)
                .AsQueryable();

            if (clienteId.HasValue)
            {
                query = query.Where(r => r.ClienteId == clienteId.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                query = query.Where(r => r.Tipo == tipo);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            var recordatorios = await query
                .OrderByDescending(r => r.FechaCreacion)
                .Select(r => new RecordatorioResponseDto
                {
                    Id = r.Id,
                    ClienteId = r.ClienteId,
                    ClienteNombre = r.Cliente != null ? r.Cliente.NombreRazonSocial : "Cliente Desconocido",
                    ClienteRtn = r.Cliente != null ? r.Cliente.Rtn : string.Empty,
                    TelefonoWhatsApp = r.TelefonoDestino ?? (r.Cliente != null ? (r.Cliente.TelefonoWhatsApp ?? r.Cliente.Telefono) : null),
                    Email = r.EmailDestino ?? (r.Cliente != null ? r.Cliente.EmailPrincipal : null),
                    CuotaMensual = r.Cliente != null ? r.Cliente.CuotaMensual : 0,
                    Tipo = r.Tipo,
                    Titulo = r.Titulo,
                    Mensaje = r.Mensaje,
                    Canal = r.Canal,
                    Estado = r.Estado,
                    FechaEnvio = r.FechaEnvio,
                    FechaCreacion = r.FechaCreacion
                })
                .ToListAsync();

            return Ok(recordatorios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecordatorioResponseDto>> GetRecordatorioById(int id)
        {
            var r = await _context.RecordatoriosClientes
                .Include(rc => rc.Cliente)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (r == null) return NotFound(new { mensaje = "Recordatorio no encontrado." });

            return Ok(new RecordatorioResponseDto
            {
                Id = r.Id,
                ClienteId = r.ClienteId,
                ClienteNombre = r.Cliente != null ? r.Cliente.NombreRazonSocial : "Cliente Desconocido",
                ClienteRtn = r.Cliente != null ? r.Cliente.Rtn : string.Empty,
                TelefonoWhatsApp = r.TelefonoDestino ?? (r.Cliente != null ? (r.Cliente.TelefonoWhatsApp ?? r.Cliente.Telefono) : null),
                Email = r.EmailDestino ?? (r.Cliente != null ? r.Cliente.EmailPrincipal : null),
                CuotaMensual = r.Cliente != null ? r.Cliente.CuotaMensual : 0,
                Tipo = r.Tipo,
                Titulo = r.Titulo,
                Mensaje = r.Mensaje,
                Canal = r.Canal,
                Estado = r.Estado,
                FechaEnvio = r.FechaEnvio,
                FechaCreacion = r.FechaCreacion
            });
        }

        [HttpPost]
        public async Task<ActionResult<RecordatorioResponseDto>> CrearRecordatorio([FromBody] RecordatorioCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null) return BadRequest(new { mensaje = "El cliente especificado no existe." });

            var recordatorio = new RecordatorioCliente
            {
                ClienteId = dto.ClienteId,
                Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? "SAR" : dto.Tipo,
                Titulo = dto.Titulo?.Trim() ?? $"Aviso {dto.Tipo}",
                Mensaje = dto.Mensaje?.Trim() ?? string.Empty,
                Canal = string.IsNullOrWhiteSpace(dto.Canal) ? "WhatsApp" : dto.Canal,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado,
                TelefonoDestino = dto.TelefonoDestino ?? cliente.TelefonoWhatsApp ?? cliente.Telefono,
                EmailDestino = dto.EmailDestino ?? cliente.EmailPrincipal
            };

            await _context.RecordatoriosClientes.AddAsync(recordatorio);
            await _context.SaveChangesAsync();

            var resp = new RecordatorioResponseDto
            {
                Id = recordatorio.Id,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.NombreRazonSocial,
                ClienteRtn = cliente.Rtn,
                TelefonoWhatsApp = recordatorio.TelefonoDestino,
                Email = recordatorio.EmailDestino,
                CuotaMensual = cliente.CuotaMensual,
                Tipo = recordatorio.Tipo,
                Titulo = recordatorio.Titulo,
                Mensaje = recordatorio.Mensaje,
                Canal = recordatorio.Canal,
                Estado = recordatorio.Estado,
                FechaEnvio = recordatorio.FechaEnvio,
                FechaCreacion = recordatorio.FechaCreacion
            };

            return CreatedAtAction(nameof(GetRecordatorioById), new { id = recordatorio.Id }, resp);
        }

        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult<RecordatorioResponseDto>> ActualizarEstado(int id, [FromBody] RecordatorioUpdateDto dto)
        {
            var r = await _context.RecordatoriosClientes
                .Include(rc => rc.Cliente)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (r == null) return NotFound(new { mensaje = "Recordatorio no encontrado." });

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                r.Estado = dto.Estado.Trim();
                if (r.Estado == "Enviado" && r.FechaEnvio == null)
                {
                    r.FechaEnvio = DateTime.UtcNow;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Mensaje))
            {
                r.Mensaje = dto.Mensaje.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Titulo))
            {
                r.Titulo = dto.Titulo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Canal))
            {
                r.Canal = dto.Canal.Trim();
            }

            await _context.SaveChangesAsync();

            return Ok(new RecordatorioResponseDto
            {
                Id = r.Id,
                ClienteId = r.ClienteId,
                ClienteNombre = r.Cliente != null ? r.Cliente.NombreRazonSocial : "Cliente",
                ClienteRtn = r.Cliente != null ? r.Cliente.Rtn : string.Empty,
                TelefonoWhatsApp = r.TelefonoDestino ?? (r.Cliente != null ? (r.Cliente.TelefonoWhatsApp ?? r.Cliente.Telefono) : null),
                Email = r.EmailDestino ?? (r.Cliente != null ? r.Cliente.EmailPrincipal : null),
                CuotaMensual = r.Cliente != null ? r.Cliente.CuotaMensual : 0,
                Tipo = r.Tipo,
                Titulo = r.Titulo,
                Mensaje = r.Mensaje,
                Canal = r.Canal,
                Estado = r.Estado,
                FechaEnvio = r.FechaEnvio,
                FechaCreacion = r.FechaCreacion
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarRecordatorio(int id)
        {
            var r = await _context.RecordatoriosClientes.FindAsync(id);
            if (r == null) return NotFound(new { mensaje = "Recordatorio no encontrado." });

            _context.RecordatoriosClientes.Remove(r);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Recordatorio eliminado con éxito." });
        }

        [HttpPost("limpiar-enviados")]
        public async Task<IActionResult> LimpiarEnviados()
        {
            var enviados = await _context.RecordatoriosClientes
                .Where(r => r.Estado == "Enviado" || r.Estado == "Respondido" || r.Estado == "Omitido")
                .ToListAsync();

            if (enviados.Count > 0)
            {
                _context.RecordatoriosClientes.RemoveRange(enviados);
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = $"Se eliminaron {enviados.Count} recordatorios gestionados." });
        }
    }
}
