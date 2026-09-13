using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Features.LibrosISV.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.LibrosISV
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LibrosIsvController : ControllerBase
    {
        private readonly LibrosIsvService _service;
        private readonly ContaFlowDbContext _context;

        public LibrosIsvController(LibrosIsvService service, ContaFlowDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet("periodo/{clienteId:int}/{anio:int}/{mes:int}")]
        [ProducesResponseType(typeof(LibroIsvDetalleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLibroIsv(int clienteId, int anio, int mes)
        {
            try
            {
                var result = await _service.GetLibroIsvAsync(clienteId, anio, mes);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("guardar")]
        [ProducesResponseType(typeof(LibroIsvDetalleDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GuardarLibroIsv([FromBody] LibroIsvGuardarDto dto)
        {
            var result = await _service.GuardarLibroIsvAsync(dto);
            return Ok(result);
        }

        [HttpGet("plantilla")]
        [AllowAnonymous]
        public async Task<IActionResult> DescargarPlantilla()
        {
            var clientes = await _context.Clientes.Where(c => c.Activo).OrderBy(c => c.NombreRazonSocial).ToListAsync();
            var bytes = _service.GenerarPlantillaCsv(clientes);
            return File(bytes, "text/csv; charset=utf-8", "Plantilla_Libros_ISV_SAR210_ContaFlow.csv");
        }

        [HttpPost("importar-masivo")]
        [ProducesResponseType(typeof(LibroIsvImportResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportarMasivo([FromBody] List<LibroIsvImportItemDto> items)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest(new { mensaje = "No se enviaron registros para procesar." });
            }

            var response = await _service.ImportarMasivoAsync(items);
            return Ok(response);
        }

        [HttpGet("exportar/{anio:int}/{mes:int}")]
        public async Task<IActionResult> ExportarResumenMes(int anio, int mes)
        {
            var periodos = await _context.PeriodosFiscalesSAR
                .Include(p => p.Cliente)
                .Where(p => p.Anio == anio && p.Mes == mes && p.Cliente.Activo)
                .OrderBy(p => p.Cliente.NombreRazonSocial)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("RTN,Cliente,Mes,Anio,Ventas 15%,Ventas 18%,Ventas Exentas,Total Ventas,Debito Fiscal,Compras 15%,Compras 18%,Compras Exentas,Total Compras,Credito Fiscal,Saldo Anterior,Retenciones,ISV a Pagar,Saldo a Favor,Estado,No Declaracion SAR");

            foreach (var p in periodos)
            {
                var ventasTotales = p.VentasGravadas15 + p.VentasGravadas18 + p.VentasExentas;
                var comprasTotales = p.ComprasGravadas15 + p.ComprasGravadas18 + p.ComprasExentas + p.ImportacionesGravadas15;
                var razon = p.Cliente.NombreRazonSocial.Replace("\"", "\"\"");

                csv.AppendLine($"{p.Cliente.Rtn},\"{razon}\",{p.Mes},{p.Anio},{p.VentasGravadas15},{p.VentasGravadas18},{p.VentasExentas},{ventasTotales},{p.TotalDebitoFiscal},{p.ComprasGravadas15},{p.ComprasGravadas18},{p.ComprasExentas},{comprasTotales},{p.TotalCreditoFiscal},{p.SaldoAFavorPeriodoAnterior},{p.RetencionesISVRecibidas},{p.ImpuestoDeterminadoPagar},{p.SaldoAFavorContribuyente},{p.Estado},{p.NumeroDeclaracionSAR}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"Libro_Resumen_ISV_SAR210_{anio}_{mes:D2}.csv");
        }
    }
}
