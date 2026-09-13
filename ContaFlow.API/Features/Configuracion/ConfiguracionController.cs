using System;
using System.Threading.Tasks;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.Configuracion
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly ContaFlowDbContext _context;

        public ConfiguracionController(ContaFlowDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ConfiguracionDespacho>> GetConfiguracion()
        {
            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new ConfiguracionDespacho();
                _context.ConfiguracionDespacho.Add(config);
                await _context.SaveChangesAsync();
            }
            return Ok(config);
        }

        [HttpPut]
        public async Task<ActionResult<ConfiguracionDespacho>> UpdateConfiguracion([FromBody] ConfiguracionDespacho request)
        {
            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new ConfiguracionDespacho();
                _context.ConfiguracionDespacho.Add(config);
            }

            config.NombreDespacho = request.NombreDespacho ?? config.NombreDespacho;
            config.NombreContadorTitular = request.NombreContadorTitular ?? config.NombreContadorTitular;
            config.ColegiacionCAH = request.ColegiacionCAH ?? config.ColegiacionCAH;
            config.RtnDespacho = request.RtnDespacho ?? config.RtnDespacho;
            config.Telefono = request.Telefono ?? config.Telefono;
            config.TelefonoWhatsApp = request.TelefonoWhatsApp ?? config.TelefonoWhatsApp;
            config.Email = request.Email ?? config.Email;
            config.Direccion = request.Direccion ?? config.Direccion;
            config.Ciudad = request.Ciudad ?? config.Ciudad;
            config.Slogan = request.Slogan ?? config.Slogan;
            config.MensajePieRecibo = request.MensajePieRecibo ?? config.MensajePieRecibo;

            if (request.LogoBase64 != null)
            {
                config.LogoBase64 = request.LogoBase64;
            }

            // Bancos
            config.Banco1Activo = request.Banco1Activo;
            config.Banco1Nombre = request.Banco1Nombre;
            config.Banco1TipoCuenta = request.Banco1TipoCuenta;
            config.Banco1Numero = request.Banco1Numero;
            config.Banco1Beneficiario = request.Banco1Beneficiario;

            config.Banco2Activo = request.Banco2Activo;
            config.Banco2Nombre = request.Banco2Nombre;
            config.Banco2TipoCuenta = request.Banco2TipoCuenta;
            config.Banco2Numero = request.Banco2Numero;
            config.Banco2Beneficiario = request.Banco2Beneficiario;

            config.Banco3Activo = request.Banco3Activo;
            config.Banco3Nombre = request.Banco3Nombre;
            config.Banco3TipoCuenta = request.Banco3TipoCuenta;
            config.Banco3Numero = request.Banco3Numero;
            config.Banco3Beneficiario = request.Banco3Beneficiario;

            config.Banco4Activo = request.Banco4Activo;
            config.Banco4Nombre = request.Banco4Nombre;
            config.Banco4TipoCuenta = request.Banco4TipoCuenta;
            config.Banco4Numero = request.Banco4Numero;
            config.Banco4Beneficiario = request.Banco4Beneficiario;

            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPost("logo")]
        public async Task<ActionResult> UpdateLogo([FromBody] LogoUploadRequest request)
        {
            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new ConfiguracionDespacho();
                _context.ConfiguracionDespacho.Add(config);
            }

            config.LogoBase64 = request.LogoBase64;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Logotipo actualizado correctamente." });
        }

        [HttpDelete("logo")]
        public async Task<ActionResult> RemoveLogo()
        {
            var config = await _context.ConfiguracionDespacho.FirstOrDefaultAsync();
            if (config != null)
            {
                config.LogoBase64 = null;
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Logotipo eliminado." });
        }
    }

    public class LogoUploadRequest
    {
        public string? LogoBase64 { get; set; }
    }
}
