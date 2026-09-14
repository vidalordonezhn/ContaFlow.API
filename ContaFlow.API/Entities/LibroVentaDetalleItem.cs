using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContaFlow.API.Entities
{
    [Table("libros_ventas_items")]
    public class LibroVentaDetalleItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PeriodoFiscalId { get; set; }

        public int? ClienteId { get; set; }

        public int Correlativo { get; set; } = 1;

        public DateTime? Fecha { get; set; }

        [MaxLength(60)]
        public string? Factura { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Exonerado { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Exento { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gravado15 { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gravado18 { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Isv15 { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Isv18 { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } = 0;

        [MaxLength(300)]
        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreadoPor { get; set; }

        public DateTime? FechaModificacion { get; set; }

        [MaxLength(100)]
        public string? ModificadoPor { get; set; }

        // Navegación
        [ForeignKey("PeriodoFiscalId")]
        public virtual PeriodoFiscalSAR? PeriodoFiscal { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }
    }
}
