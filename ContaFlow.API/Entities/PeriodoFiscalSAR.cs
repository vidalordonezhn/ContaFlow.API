using System;

namespace ContaFlow.API.Entities
{
    public class PeriodoFiscalSAR : AuditableEntity
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int Mes { get; set; } // 1 a 12
        public int Anio { get; set; } // Ej: 2026
        
        // Control de Recepción de Documentos (antes del día 10)
        public bool FacturasRecibidas { get; set; } = false;
        public DateTime? FechaRecepcionFacturas { get; set; }
        public int CantidadFacturasVenta { get; set; } = 0;
        public int CantidadFacturasCompra { get; set; } = 0;
        public string? NotasDocumentos { get; set; }

        // Libro de Ventas (Débito Fiscal)
        public decimal VentasGravadas15 { get; set; } = 0;
        public decimal VentasGravadas18 { get; set; } = 0;
        public decimal VentasExentas { get; set; } = 0;
        public decimal IsvDebito15 { get; set; } = 0;
        public decimal IsvDebito18 { get; set; } = 0;
        public decimal TotalDebitoFiscal { get; set; } = 0;

        // Libro de Compras (Crédito Fiscal)
        public decimal ComprasGravadas15 { get; set; } = 0;
        public decimal ComprasGravadas18 { get; set; } = 0;
        public decimal ComprasExentas { get; set; } = 0;
        public decimal ImportacionesGravadas15 { get; set; } = 0;
        public decimal IsvCredito15 { get; set; } = 0;
        public decimal IsvCredito18 { get; set; } = 0;
        public decimal TotalCreditoFiscal { get; set; } = 0;

        // Liquidación SAR-210 & Hoja de Trabajo
        public decimal SaldoAFavorPeriodoAnterior { get; set; } = 0;
        public decimal RetencionesISVRecibidas { get; set; } = 0;
        public decimal Retenciones15 { get; set; } = 0;
        public decimal Retenciones18 { get; set; } = 0;
        public decimal ServiciosProfesionales { get; set; } = 0;
        public decimal ImpuestoDeterminadoPagar { get; set; } = 0;
        public decimal SaldoAFavorContribuyente { get; set; } = 0;

        // Control de Liquidación / Declaración SAR
        public bool LiquidadoSAR { get; set; } = false;
        public DateTime? FechaLiquidacion { get; set; }
        public decimal? MontoImpuestoISV { get; set; }
        public decimal? MontoRetenciones { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, EnProceso, Liquidado, Declarado

        public ICollection<LibroDetalleItem> DetalleItems { get; set; } = new List<LibroDetalleItem>();
        public ICollection<LibroVentaDetalleItem> VentasDetalleItems { get; set; } = new List<LibroVentaDetalleItem>();
        public ICollection<LibroCompraDetalleItem> ComprasDetalleItems { get; set; } = new List<LibroCompraDetalleItem>();
    }
}
