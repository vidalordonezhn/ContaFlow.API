using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContaFlow.API.Features.Clientes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContaFlow.API.Features.Clientes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly ClientesService _clientesService;

        public ClientesController(ClientesService clientesService)
        {
            _clientesService = clientesService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ClienteResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _clientesService.GetClientesAsync();
            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCliente(int id)
        {
            try
            {
                var cliente = await _clientesService.GetClienteByIdAsync(id);
                return Ok(cliente);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var nuevoCliente = await _clientesService.CrearClienteAsync(dto);
                return Ok(nuevoCliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var clienteActualizado = await _clientesService.ActualizarClienteAsync(id, dto);
                return Ok(clienteActualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _clientesService.ToggleStatusAsync(id);
                return Ok(new { mensaje = "Estado del cliente actualizado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:int}/expediente")]
        [ProducesResponseType(typeof(ExpedienteFiscalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExpedienteFiscal(int id)
        {
            try
            {
                var expediente = await _clientesService.GetExpedienteFiscalAsync(id);
                return Ok(expediente);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // ==========================================
        // IMPORTACIÓN MASIVA DE CLIENTES
        // ==========================================

        [HttpPost("importar-masivo")]
        [ProducesResponseType(typeof(ClienteImportResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportarMasivo([FromBody] List<ClienteImportItemDto> items)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest(new { mensaje = "No se enviaron clientes para procesar." });
            }

            var result = await _clientesService.ImportarClientesMasivoAsync(items);
            return Ok(result);
        }

        [HttpGet("plantilla")]
        [AllowAnonymous]
        public IActionResult DescargarPlantilla()
        {
            var bytes = _clientesService.GenerarPlantillaClientesCsv();
            return File(bytes, "text/csv; charset=utf-8", "Plantilla_Clientes_ContaFlow.csv");
        }
    }
}
