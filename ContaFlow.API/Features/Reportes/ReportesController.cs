using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContaFlow.API.Data;
using System.Globalization;

namespace ContaFlow.API.Features.Reportes;

public class ReporteFinancieroResponse
{
    public decimal TotalRecaudadoHistorico { get; set; }
    public decimal TotalFacturableMensual { get; set; }
    public decimal RecaudadoMesActual { get; set; }
    public decimal PendienteCobroMesActual { get; set; }
    public double PorcentajeCobranzaMesActual { get; set; }

    public int TotalClientesActivos { get; set; }
    public int ClientesAlDiaCount { get; set; }
    public int ClientesMorososCount { get; set; }
    public double PorcentajeClientesAlDia { get; set; }

    public List<IngresoMensualItem> HistoricoIngresos { get; set; } = new();
    public List<DistribucionItem> DistribucionPorTipoPersona { get; set; } = new();
    public List<DistribucionItem> DistribucionPorRubro { get; set; } = new();
}

public class IngresoMensualItem
{
    public string MesNombre { get; set; } = string.Empty;
    public int MesNumero { get; set; }
    public int Anio { get; set; }
    public decimal MontoTotal { get; set; }
    public int CantidadPagos { get; set; }
}

public class DistribucionItem
{
    public string Etiqueta { get; set; } = string.Empty;
    public int CantidadClientes { get; set; }
    public decimal TotalCuotas { get; set; }
    public double Porcentaje { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly ContaFlowDbContext _context;

    public ReportesController(ContaFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet("financieros")]
    public async Task<ActionResult<ReporteFinancieroResponse>> GetReporteFinanciero()
    {
        var hoy = DateTime.UtcNow;
        var nombreMeses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        var mesActualNombre = $"{nombreMeses[hoy.Month - 1]} {hoy.Year}";

        // Clientes activos
        var clientesActivos = await _context.Clientes
            .Where(c => c.Activo)
            .ToListAsync();

        var totalClientesActivos = clientesActivos.Count;
        var totalFacturableMensual = clientesActivos.Sum(c => c.CuotaMensual);

        // Todos los pagos
        var todosLosPagos = await _context.PagosHonorarios.ToListAsync();
        var totalRecaudadoHistorico = todosLosPagos.Sum(p => p.Monto);

        // Pagos del mes actual
        var pagosMesActual = todosLosPagos
            .Where(p => !string.IsNullOrEmpty(p.MesAplicado) && p.MesAplicado.Equals(mesActualNombre, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var recaudadoMesActual = pagosMesActual.Sum(p => p.Monto);
        var pendienteCobroMesActual = Math.Max(0, totalFacturableMensual - recaudadoMesActual);
        var porcentajeCobranzaMesActual = totalFacturableMensual > 0
            ? Math.Round((double)(recaudadoMesActual / totalFacturableMensual) * 100.0, 1)
            : 0;

        // Estado de cartera (Al día vs Morosos)
        var clienteIdsPagadosEsteMes = pagosMesActual.Select(p => p.ClienteId).ToHashSet();
        var clientesAlDiaCount = clientesActivos.Count(c => clienteIdsPagadosEsteMes.Contains(c.Id));
        var clientesMorososCount = totalClientesActivos - clientesAlDiaCount;
        var porcentajeAlDia = totalClientesActivos > 0
            ? Math.Round(((double)clientesAlDiaCount / totalClientesActivos) * 100.0, 1)
            : 100.0;

        // Histórico de ingresos por mes (últimos 6 meses)
        var historico = new List<IngresoMensualItem>();
        for (int i = 5; i >= 0; i--)
        {
            var fechaRef = hoy.AddMonths(-i);
            var mesNom = $"{nombreMeses[fechaRef.Month - 1]} {fechaRef.Year}";
            var pagosDelMes = todosLosPagos.Where(p => 
                (!string.IsNullOrEmpty(p.MesAplicado) && p.MesAplicado.Equals(mesNom, StringComparison.OrdinalIgnoreCase)) ||
                (p.FechaPago.Month == fechaRef.Month && p.FechaPago.Year == fechaRef.Year)
            ).ToList();

            historico.Add(new IngresoMensualItem
            {
                MesNombre = nombreMeses[fechaRef.Month - 1].Substring(0, 3) + " " + fechaRef.Year,
                MesNumero = fechaRef.Month,
                Anio = fechaRef.Year,
                MontoTotal = pagosDelMes.Sum(p => p.Monto),
                CantidadPagos = pagosDelMes.Count
            });
        }

        // Distribución por Tipo de Persona
        var distribucionPersona = clientesActivos
            .GroupBy(c => c.TipoPersona)
            .Select(g => new DistribucionItem
            {
                Etiqueta = g.Key == "Juridica" ? "Personas Jurídicas" : "Personas Naturales",
                CantidadClientes = g.Count(),
                TotalCuotas = g.Sum(c => c.CuotaMensual),
                Porcentaje = totalClientesActivos > 0 ? Math.Round(((double)g.Count() / totalClientesActivos) * 100.0, 1) : 0
            }).ToList();

        // Distribución por Rubro
        var distribucionRubro = clientesActivos
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Rubro) ? "Otros / Varios" : c.Rubro.Trim())
            .OrderByDescending(g => g.Sum(c => c.CuotaMensual))
            .Take(5)
            .Select(g => new DistribucionItem
            {
                Etiqueta = g.Key,
                CantidadClientes = g.Count(),
                TotalCuotas = g.Sum(c => c.CuotaMensual),
                Porcentaje = totalFacturableMensual > 0 ? Math.Round((double)(g.Sum(c => c.CuotaMensual) / totalFacturableMensual) * 100.0, 1) : 0
            }).ToList();

        var response = new ReporteFinancieroResponse
        {
            TotalRecaudadoHistorico = totalRecaudadoHistorico,
            TotalFacturableMensual = totalFacturableMensual,
            RecaudadoMesActual = recaudadoMesActual,
            PendienteCobroMesActual = pendienteCobroMesActual,
            PorcentajeCobranzaMesActual = porcentajeCobranzaMesActual,
            TotalClientesActivos = totalClientesActivos,
            ClientesAlDiaCount = clientesAlDiaCount,
            ClientesMorososCount = clientesMorososCount,
            PorcentajeClientesAlDia = porcentajeAlDia,
            HistoricoIngresos = historico,
            DistribucionPorTipoPersona = distribucionPersona,
            DistribucionPorRubro = distribucionRubro
        };

        return Ok(response);
    }
}
