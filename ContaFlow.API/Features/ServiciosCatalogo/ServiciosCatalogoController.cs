using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.ServiciosCatalogo.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.ServiciosCatalogo
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiciosCatalogoController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public ServiciosCatalogoController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServicioCatalogoResponseDto>>> GetServicios()
        {
            // Si la tabla está vacía, insertar datos iniciales estándar
            if (!await _context.ServiciosCatalogo.AnyAsync())
            {
                var defaults = new List<ServicioCatalogo>
                {
                    new() { Nombre = "Talonario de Facturas", DescripcionDefault = "Talonario de facturas fiscales de 3 copias", PrecioDefault = 350, Categoria = "Talonarios", Activo = true },
                    new() { Nombre = "Constancia Electrónica", DescripcionDefault = "Emisión de constancia electrónica fiscal ante el SAR", PrecioDefault = 250, Categoria = "SAR", Activo = true },
                    new() { Nombre = "Pagos a Cuenta SAR", DescripcionDefault = "Cálculo y presentación de cuota trimestral de Pagos a Cuenta", PrecioDefault = 400, Categoria = "SAR", Activo = true },
                    new() { Nombre = "Impuesto sobre la Renta", DescripcionDefault = "Declaración jurada y liquidación anual de ISR", PrecioDefault = 800, Categoria = "Declaraciones", Activo = true },
                    new() { Nombre = "Controles Tributarios", DescripcionDefault = "Revisión y auditoría de control tributario mensual", PrecioDefault = 500, Categoria = "Auditoría", Activo = true },
                    new() { Nombre = "Honorarios Mensuales", DescripcionDefault = "Asesoría contable y cumplimiento tributario mensual", PrecioDefault = 600, Categoria = "Honorarios", Activo = true },
                    new() { Nombre = "Trámites en Línea SAR", DescripcionDefault = "Gestión de solicitudes y trámites en plataforma SAR", PrecioDefault = 300, Categoria = "SAR", Activo = true }
                };
                await _context.ServiciosCatalogo.AddRangeAsync(defaults);
                await _context.SaveChangesAsync();
            }

            var items = await _context.ServiciosCatalogo
                .OrderBy(s => s.Nombre)
                .Select(s => new ServicioCatalogoResponseDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    DescripcionDefault = s.DescripcionDefault,
                    PrecioDefault = s.PrecioDefault,
                    Categoria = s.Categoria,
                    Activo = s.Activo,
                    FechaCreacion = s.FechaCreacion
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServicioCatalogoResponseDto>> GetServicioById(int id)
        {
            var item = await _context.ServiciosCatalogo.FindAsync(id);
            if (item == null) return NotFound(new { mensaje = "Producto o servicio no encontrado." });

            return Ok(new ServicioCatalogoResponseDto
            {
                Id = item.Id,
                Nombre = item.Nombre,
                DescripcionDefault = item.DescripcionDefault,
                PrecioDefault = item.PrecioDefault,
                Categoria = item.Categoria,
                Activo = item.Activo,
                FechaCreacion = item.FechaCreacion
            });
        }

        [HttpPost]
        public async Task<ActionResult<ServicioCatalogoResponseDto>> CrearServicio([FromBody] ServicioCatalogoCreateDto dto)
        {
            var servicio = new ServicioCatalogo
            {
                Nombre = dto.Nombre.Trim(),
                DescripcionDefault = dto.DescripcionDefault?.Trim(),
                PrecioDefault = dto.PrecioDefault,
                Categoria = !string.IsNullOrWhiteSpace(dto.Categoria) ? dto.Categoria.Trim() : "General",
                Activo = true
            };

            await _context.ServiciosCatalogo.AddAsync(servicio);
            await _context.SaveChangesAsync();

            var resp = new ServicioCatalogoResponseDto
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                DescripcionDefault = servicio.DescripcionDefault,
                PrecioDefault = servicio.PrecioDefault,
                Categoria = servicio.Categoria,
                Activo = servicio.Activo,
                FechaCreacion = servicio.FechaCreacion
            };

            return CreatedAtAction(nameof(GetServicioById), new { id = servicio.Id }, resp);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ServicioCatalogoResponseDto>> ActualizarServicio(int id, [FromBody] ServicioCatalogoUpdateDto dto)
        {
            var item = await _context.ServiciosCatalogo.FindAsync(id);
            if (item == null) return NotFound(new { mensaje = "Producto o servicio no encontrado." });

            item.Nombre = dto.Nombre.Trim();
            item.DescripcionDefault = dto.DescripcionDefault?.Trim();
            item.PrecioDefault = dto.PrecioDefault;
            item.Categoria = !string.IsNullOrWhiteSpace(dto.Categoria) ? dto.Categoria.Trim() : item.Categoria;
            item.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new ServicioCatalogoResponseDto
            {
                Id = item.Id,
                Nombre = item.Nombre,
                DescripcionDefault = item.DescripcionDefault,
                PrecioDefault = item.PrecioDefault,
                Categoria = item.Categoria,
                Activo = item.Activo,
                FechaCreacion = item.FechaCreacion
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarServicio(int id)
        {
            var item = await _context.ServiciosCatalogo.FindAsync(id);
            if (item == null) return NotFound(new { mensaje = "Producto o servicio no encontrado." });

            _context.ServiciosCatalogo.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto o servicio eliminado del catálogo." });
        }
    }
}
