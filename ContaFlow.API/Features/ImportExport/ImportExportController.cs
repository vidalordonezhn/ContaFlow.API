using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using System.Text;
using System.Security.Claims;

namespace ContaFlow.API.Features.ImportExport;

public class ClienteImportItemDto
{
    public string Rtn { get; set; } = string.Empty;
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string TipoPersona { get; set; } = "Juridica";
    public string? Rubro { get; set; }
    public string? EmailPrincipal { get; set; }
    public string? TelefonoWhatsApp { get; set; }
    public string? Direccion { get; set; }
    public decimal CuotaMensual { get; set; }
    public int DiaCobro { get; set; } = 5;
    public string? Notas { get; set; }
}

public class ImportacionClientesResponse
{
    public int TotalProcesados { get; set; }
    public int TotalInsertados { get; set; }
    public int TotalOmitidosPorDuplicado { get; set; }
    public int TotalErrores { get; set; }
    public List<string> Mensajes { get; set; } = new();
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportExportController : ControllerBase
{
    private readonly ContaFlowDbContext _context;

    public ImportExportController(ContaFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet("plantilla-clientes")]
    [AllowAnonymous]
    public IActionResult DescargarPlantillaClientes()
    {
        var csv = new StringBuilder();
        // UTF-8 BOM for Excel compatibility
        csv.AppendLine("RTN,NombreRazonSocial,NombreComercial,TipoPersona,Rubro,CuotaMensual,DiaCobro,TelefonoWhatsApp,EmailPrincipal,Direccion");
        csv.AppendLine("08011990123456,Inversiones del Norte S.A. de C.V.,Norte Imports,Juridica,Comercio,3500.00,5,50499887766,contacto@norteimports.hn,Colonia Alameda Tegucigalpa");
        csv.AppendLine("05011985654321,Lic. Roberto Mendoza,Mendoza Consultores,Natural,Servicios Profesionales,2000.00,10,50433221100,roberto@mendoza.hn,San Pedro Sula Cortes");
        csv.AppendLine("08011978112233,Farmacia La Esperanza S. de R.L.,La Esperanza,Juridica,Farmacia / Salud,4500.00,5,50488776655,esperanza@farmacias.hn,Barrio Abajo Tegucigalpa");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", "Plantilla_Importacion_Clientes_ContaFlow.csv");
    }

    [HttpPost("importar-clientes")]
    public async Task<ActionResult<ImportacionClientesResponse>> ImportarClientes([FromBody] List<ClienteImportItemDto> items)
    {
        if (items == null || items.Count == 0)
        {
            return BadRequest(new { mensaje = "No se recibieron clientes para importar." });
        }

        var rtnsExistentes = await _context.Clientes
            .Select(c => c.Rtn.Trim().ToUpper())
            .ToListAsync();

        var setRtns = new HashSet<string>(rtnsExistentes);
        var response = new ImportacionClientesResponse
        {
            TotalProcesados = items.Count
        };

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
        var hoy = DateTime.UtcNow;

        var nuevosClientes = new List<Cliente>();

        foreach (var item in items)
        {
            var cleanRtn = (item.Rtn ?? string.Empty).Trim().Replace("-", "").ToUpper();
            if (string.IsNullOrWhiteSpace(cleanRtn) || string.IsNullOrWhiteSpace(item.NombreRazonSocial))
            {
                response.TotalErrores++;
                response.Mensajes.Add($"Fila omitida: RTN o Nombre en blanco ({item.NombreRazonSocial}).");
                continue;
            }

            if (setRtns.Contains(cleanRtn))
            {
                response.TotalOmitidosPorDuplicado++;
                response.Mensajes.Add($"Omitido por RTN duplicado: {cleanRtn} ({item.NombreRazonSocial}).");
                continue;
            }

            var cliente = new Cliente
            {
                Rtn = cleanRtn,
                NombreRazonSocial = item.NombreRazonSocial.Trim(),
                NombreComercial = string.IsNullOrWhiteSpace(item.NombreComercial) ? null : item.NombreComercial.Trim(),
                TipoPersona = item.TipoPersona?.Trim() == "Natural" ? "Natural" : "Juridica",
                Rubro = string.IsNullOrWhiteSpace(item.Rubro) ? "General" : item.Rubro.Trim(),
                CuotaMensual = item.CuotaMensual > 0 ? item.CuotaMensual : 1500,
                DiaCobro = item.DiaCobro > 0 && item.DiaCobro <= 31 ? item.DiaCobro : 5,
                TelefonoWhatsApp = string.IsNullOrWhiteSpace(item.TelefonoWhatsApp) ? null : item.TelefonoWhatsApp.Trim(),
                EmailPrincipal = string.IsNullOrWhiteSpace(item.EmailPrincipal) ? null : item.EmailPrincipal.Trim(),
                Direccion = string.IsNullOrWhiteSpace(item.Direccion) ? null : item.Direccion.Trim(),
                Notas = item.Notas,
                Activo = true,
                FechaCreacion = hoy,
                CreadoPor = userName
            };

            nuevosClientes.Add(cliente);
            setRtns.Add(cleanRtn);
        }

        if (nuevosClientes.Count > 0)
        {
            await _context.Clientes.AddRangeAsync(nuevosClientes);
            await _context.SaveChangesAsync();

            // Generar periodo SAR inicial para cada nuevo cliente
            var periodosSAR = nuevosClientes.Select(c => new PeriodoFiscalSAR
            {
                ClienteId = c.Id,
                Mes = hoy.Month,
                Anio = hoy.Year,
                FacturasRecibidas = false,
                LiquidadoSAR = false,
                Estado = "Pendiente",
                FechaCreacion = hoy,
                CreadoPor = userName
            }).ToList();

            await _context.PeriodosFiscalesSAR.AddRangeAsync(periodosSAR);
            await _context.SaveChangesAsync();
        }

        response.TotalInsertados = nuevosClientes.Count;
        return Ok(response);
    }

    [HttpGet("exportar-clientes")]
    public async Task<IActionResult> ExportarClientes()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.NombreRazonSocial)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("ID,RTN,Razón Social,Nombre Comercial,Tipo Persona,Rubro,Cuota Mensual (HNL),Día Cobro,WhatsApp,Email,Estado,Fecha Registro");

        foreach (var c in clientes)
        {
            var razon = EscapeCsv(c.NombreRazonSocial);
            var comercial = EscapeCsv(c.NombreComercial ?? "");
            var rubro = EscapeCsv(c.Rubro ?? "");
            var estado = c.Activo ? "Activo" : "Inactivo";
            var fecha = c.FechaCreacion.ToString("yyyy-MM-dd");

            csv.AppendLine($"{c.Id},{c.Rtn},{razon},{comercial},{c.TipoPersona},{rubro},{c.CuotaMensual},{c.DiaCobro},{c.TelefonoWhatsApp},{c.EmailPrincipal},{estado},{fecha}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Directorio_Clientes_ContaFlow_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("exportar-pagos")]
    public async Task<IActionResult> ExportarPagos()
    {
        var pagos = await _context.PagosHonorarios
            .Include(p => p.Cliente)
            .Include(p => p.Recibo)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("N° Recibo,Cliente,RTN,Mes Aplicado,Monto (HNL),Método Pago,Referencia,Fecha Pago,Registrado Por");

        foreach (var p in pagos)
        {
            var clienteNombre = EscapeCsv(p.Cliente?.NombreRazonSocial ?? "Desconocido");
            var reciboNum = p.Recibo?.NumeroRecibo ?? "N/A";
            var refPago = EscapeCsv(p.ReferenciaBancaria ?? "");
            var mes = EscapeCsv(p.MesAplicado);
            var fecha = p.FechaPago.ToString("yyyy-MM-dd HH:mm");
            var creadoPor = EscapeCsv(p.CreadoPor ?? "Admin");

            csv.AppendLine($"{reciboNum},{clienteNombre},{p.Cliente?.Rtn},{mes},{p.Monto},{p.MetodoPago},{refPago},{fecha},{creadoPor}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Historial_Pagos_ContaFlow_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
