using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContaFlow.API.Features.SARControl.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaFlow.API.Features.SARControl
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SARControlController : ControllerBase
    {
        private readonly SARControlService _sarService;

        public SARControlController(SARControlService sarService)
        {
            _sarService = sarService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PeriodoSARResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeriodos([FromQuery] int? mes, [FromQuery] int? anio)
        {
            var ahora = DateTime.UtcNow;
            var targetMes = mes ?? ahora.Month;
            var targetAnio = anio ?? ahora.Year;

            var periodos = await _sarService.GetPeriodosPorMesAsync(targetMes, targetAnio);
            return Ok(periodos);
        }

        [HttpGet("resumen")]
        [ProducesResponseType(typeof(SARResumenMensualDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResumen([FromQuery] int? mes, [FromQuery] int? anio)
        {
            var ahora = DateTime.UtcNow;
            var targetMes = mes ?? ahora.Month;
            var targetAnio = anio ?? ahora.Year;

            var resumen = await _sarService.GetResumenMensualAsync(targetMes, targetAnio);
            return Ok(resumen);
        }

        [HttpPatch("{id:int}/recepcion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarcarRecepcion(int id, [FromBody] MarcarRecepcionDto dto)
        {
            try
            {
                await _sarService.MarcarFacturasRecibidasAsync(id, dto);
                return Ok(new { mensaje = "Recepción de facturas actualizada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/liquidacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarLiquidacion(int id, [FromBody] RegistrarLiquidacionSARDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _sarService.RegistrarLiquidacionSARAsync(id, dto);
                return Ok(new { mensaje = "Liquidación SAR registrada con éxito." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}
