using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GeoController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public GeoController(ContaFlowDbContext context)
        {
            _context = context;
        }

        public record MunicipioDto(int Id, int DepartamentoId, string Codigo, string Nombre);
        public record DepartamentoDto(int Id, string Codigo, string Nombre, string? Cabecera, List<MunicipioDto> Municipios);

        [HttpGet("departamentos")]
        public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetDepartamentos()
        {
            var departamentos = await _context.Departamentos
                .Include(d => d.Municipios)
                .OrderBy(d => d.Codigo)
                .Select(d => new DepartamentoDto(
                    d.Id,
                    d.Codigo,
                    d.Nombre,
                    d.Cabecera,
                    d.Municipios.OrderBy(m => m.Codigo).Select(m => new MunicipioDto(m.Id, m.DepartamentoId, m.Codigo, m.Nombre)).ToList()
                ))
                .ToListAsync();

            return Ok(departamentos);
        }

        [HttpGet("departamentos/{departamentoId:int}/municipios")]
        public async Task<ActionResult<IEnumerable<MunicipioDto>>> GetMunicipiosPorDepartamento(int departamentoId)
        {
            var municipios = await _context.Municipios
                .Where(m => m.DepartamentoId == departamentoId)
                .OrderBy(m => m.Codigo)
                .Select(m => new MunicipioDto(m.Id, m.DepartamentoId, m.Codigo, m.Nombre))
                .ToListAsync();

            return Ok(municipios);
        }

        [HttpGet("municipios")]
        public async Task<ActionResult<IEnumerable<MunicipioDto>>> GetTodosMunicipios()
        {
            var municipios = await _context.Municipios
                .OrderBy(m => m.Codigo)
                .Select(m => new MunicipioDto(m.Id, m.DepartamentoId, m.Codigo, m.Nombre))
                .ToListAsync();

            return Ok(municipios);
        }
    }
}
