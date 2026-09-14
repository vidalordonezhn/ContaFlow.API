using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RubrosController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public RubrosController(ContaFlowDbContext context)
        {
            _context = context;
        }

        public record RubroDto(int Id, string Nombre, string? Descripcion, bool Activo);
        public record RubroCreateDto(string Nombre, string? Descripcion);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RubroDto>>> GetRubros()
        {
            var rubros = await _context.Rubros
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .Select(r => new RubroDto(r.Id, r.Nombre, r.Descripcion, r.Activo))
                .ToListAsync();

            return Ok(rubros);
        }

        [HttpPost]
        public async Task<ActionResult<RubroDto>> CrearRubro([FromBody] RubroCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return BadRequest(new { message = "El nombre del rubro es requerido." });
            }

            var existe = await _context.Rubros.AnyAsync(r => r.Nombre.ToLower() == dto.Nombre.Trim().ToLower());
            if (existe)
            {
                return BadRequest(new { message = "Ya existe un rubro con ese nombre." });
            }

            var rubro = new Rubro
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Activo = true
            };

            _context.Rubros.Add(rubro);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRubros), new { id = rubro.Id }, new RubroDto(rubro.Id, rubro.Nombre, rubro.Descripcion, rubro.Activo));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarRubro(int id)
        {
            var rubro = await _context.Rubros.FindAsync(id);
            if (rubro == null)
            {
                return NotFound(new { message = "Rubro no encontrado." });
            }

            // Soft-delete
            rubro.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
