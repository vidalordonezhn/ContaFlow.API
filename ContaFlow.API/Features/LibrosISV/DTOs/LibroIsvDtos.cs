using System;
using System.Collections.Generic;

namespace ContaFlow.API.Features.LibrosISV.DTOs
{
    public class LibroIsvDetalleDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteRtn { get; set; } = string.Empty;
        public string? ClienteContrasenaSAR { get; set; }
        public decimal CuotaHonorarios { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string MesNombre { get; set; } = string.Empty;

        // Documentos
        public bool FacturasRecibidas { get; set; }
        public int CantidadFacturasVenta { get; set; }
        public int CantidadFacturasCompra { get; set; }

        // Libro de Ventas (Débito Fiscal)
        public decimal VentasGravadas15 { get; set; }
        public decimal VentasGravadas18 { get; set; }
        public decimal VentasExentas { get; set; }
        public decimal IsvDebito15 { get; set; }
        public decimal IsvDebito18 { get; set; }
        public decimal TotalVentasNetas => VentasGravadas15 + VentasGravadas18 + VentasExentas;
        public decimal TotalDebitoFiscal { get; set; }

        // Libro de Compras (Crédito Fiscal)
        public decimal ComprasGravadas15 { get; set; }
        public decimal ComprasGravadas18 { get; set; }
        public decimal ComprasExentas { get; set; }
        public decimal ImportacionesGravadas15 { get; set; }
        public decimal IsvCredito15 { get; set; }
        public decimal IsvCredito18 { get; set; }
        public decimal TotalComprasNetas => ComprasGravadas15 + ComprasGravadas18 + ComprasExentas + ImportacionesGravadas15;
        public decimal TotalCreditoFiscal { get; set; }

        // Liquidación SAR-210
        public decimal SaldoAFavorPeriodoAnterior { get; set; }
        public decimal RetencionesISVRecibidas { get; set; }
        public decimal ImpuestoDeterminadoPagar { get; set; }
        public decimal SaldoAFavorContribuyente { get; set; }

        // Estado SAR
        public bool LiquidadoSAR { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }

    public class LibroIsvGuardarDto
    {
        public int ClienteId { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }

        public decimal VentasGravadas15 { get; set; }
        public decimal VentasGravadas18 { get; set; }
        public decimal VentasExentas { get; set; }

        public decimal ComprasGravadas15 { get; set; }
        public decimal ComprasGravadas18 { get; set; }
        public decimal ComprasExentas { get; set; }
        public decimal ImportacionesGravadas15 { get; set; }

        public decimal SaldoAFavorPeriodoAnterior { get; set; }
        public decimal RetencionesISVRecibidas { get; set; }

        public bool MarcarComoLiquidado { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
    }

    // DTOs para Partidas Detalladas del Libro Diario / Hoja de Trabajo
    public class LibroPartidaItemDto
    {
        public int Id { get; set; }
        public int Correlativo { get; set; }
        public string? Fecha { get; set; } // yyyy-MM-dd
        public string? Proveedor { get; set; }

        // Compras
        public decimal ComprasExentas { get; set; }
        public decimal ComprasGravadas { get; set; }
        public decimal IsvCompras15 { get; set; }
        public string? FacturaNumero { get; set; }

        // Ventas
        public decimal VentasExentas { get; set; }
        public decimal VentasGravadas { get; set; }
        public decimal IsvVentas15 { get; set; }
        public string? Notas { get; set; }
    }

    public class LibroDetalleCompletoDto
    {
        public int PeriodoFiscalId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteRtn { get; set; } = string.Empty;
        public string? ClienteContrasenaSAR { get; set; }
        public decimal CuotaHonorarios { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string MesNombre { get; set; } = string.Empty;

        public List<LibroPartidaItemDto> Items { get; set; } = new();

        // Totales calculados
        public decimal TotalComprasExentas { get; set; }
        public decimal TotalComprasGravadas { get; set; }
        public decimal TotalIsvCompras15 { get; set; }

        public decimal TotalVentasExentas { get; set; }
        public decimal TotalVentasGravadas { get; set; }
        public decimal TotalIsvVentas15 { get; set; }

        // Resumen de liquidación
        public decimal ImpuestoCompras => TotalIsvCompras15;
        public decimal ImpuestoVentas => TotalIsvVentas15;
        public decimal ImpuestoAPagar => Math.Max(0, ImpuestoVentas - ImpuestoCompras);
        public decimal SaldoAFavor => Math.Max(0, ImpuestoCompras - ImpuestoVentas);
        public decimal ServiciosProfesionales { get; set; }
        public decimal TotalAPagarLps => ImpuestoAPagar + ServiciosProfesionales;
    }

    public class GuardarLibroDetallePartidasRequest
    {
        public int ClienteId { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal? ServiciosProfesionales { get; set; }
        public List<LibroPartidaItemDto> Items { get; set; } = new();
    }

    public class LibroIsvImportItemDto
    {
        public string Rtn { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Anio { get; set; }

        public decimal VentasGravadas15 { get; set; }
        public decimal VentasGravadas18 { get; set; }
        public decimal VentasExentas { get; set; }

        public decimal ComprasGravadas15 { get; set; }
        public decimal ComprasGravadas18 { get; set; }
        public decimal ComprasExentas { get; set; }
        public decimal ImportacionesGravadas15 { get; set; }

        public decimal SaldoAFavorPeriodoAnterior { get; set; }
        public decimal RetencionesISVRecibidas { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
    }

    public class LibroIsvImportResponseDto
    {
        public int TotalProcesados { get; set; }
        public int TotalGuardados { get; set; }
        public int TotalErrores { get; set; }
        public List<string> Mensajes { get; set; } = new();
    }
}
