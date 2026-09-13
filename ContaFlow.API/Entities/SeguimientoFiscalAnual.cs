using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Entities
{
    public class SeguimientoFiscalAnual : AuditableEntity
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int Anio { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoObligacion { get; set; } = "ISR_ANUAL"; 
        // Tipos: "RETENCIONES_ANUAL" (31 Ene), "ISR_ANUAL" (30 Abr), "PAGO_CUENTA_1" (30 Jun), "PAGO_CUENTA_2" (30 Sep), "PAGO_CUENTA_3" (31 Dic)

        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = "Pendiente"; // "Pendiente", "EnProceso", "Declarado", "NoAplica"

        public decimal? MontoDeclarado { get; set; }

        [MaxLength(100)]
        public string? NumeroDeclaracionSAR { get; set; }

        public DateTime? FechaCumplimiento { get; set; }

        [MaxLength(300)]
        public string? Observaciones { get; set; }
    }
}
