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
