using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContaFlow.API.Features.Pagos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaFlow.API.Features.Pagos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly PagosService _pagosService;

        public PagosController(PagosService pagosService)
        {
            _pagosService = pagosService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PagoResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _pagosService.GetPagosAsync();
            return Ok(pagos);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PagoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var nuevoPago = await _pagosService.RegistrarPagoAsync(dto);
                return Ok(nuevoPago);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}
