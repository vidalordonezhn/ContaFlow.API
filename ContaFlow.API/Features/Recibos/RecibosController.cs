using System;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ContaFlow.API.Features.Recibos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecibosController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public RecibosController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarReciboPdf(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var recibo = await _context.Recibos
                .Include(r => r.PagoHonorario)
                .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recibo == null)
            {
                return NotFound(new { mensaje = "Recibo no encontrado." });
            }

            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            var documento = new ReciboDocument(recibo, config);
            var pdfBytes = documento.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"{recibo.NumeroRecibo}.pdf");
        }
    }
}
