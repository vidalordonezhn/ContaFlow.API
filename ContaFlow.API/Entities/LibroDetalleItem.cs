using System;

namespace ContaFlow.API.Entities
{
    public class LibroDetalleItem : AuditableEntity
    {
        public int Id { get; set; }
        public int PeriodoFiscalId { get; set; }
        public PeriodoFiscalSAR? PeriodoFiscal { get; set; }

        public int Correlativo { get; set; } = 1;
        public DateTime? Fecha { get; set; }
        public string? Proveedor { get; set; } = string.Empty;

        // Compras
        public decimal ComprasExentas { get; set; } = 0.00m;
        public decimal ComprasGravadas { get; set; } = 0.00m;
        public decimal IsvCompras15 { get; set; } = 0.00m;
        public string? FacturaNumero { get; set; }

        // Ventas
        public decimal VentasExentas { get; set; } = 0.00m;
        public decimal VentasGravadas { get; set; } = 0.00m;
        public decimal IsvVentas15 { get; set; } = 0.00m;

        public string? Notas { get; set; }
    }
}
