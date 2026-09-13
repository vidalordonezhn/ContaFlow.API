using System;

namespace ContaFlow.API.Entities
{
    public class Recibo : AuditableEntity
    {
        public int Id { get; set; }
        public int PagoHonorarioId { get; set; }
        public PagoHonorario PagoHonorario { get; set; } = null!;

        public string NumeroRecibo { get; set; } = string.Empty; // Ej: "REC-2026-0001"
        public string? NumeroFiscal { get; set; } // Ej: "000-001-01-00000001"
        public int? AutorizacionCAIId { get; set; }
        public AutorizacionCAI? AutorizacionCAI { get; set; }
        public string? Cai { get; set; }
        public string? RangoAutorizado { get; set; }
        public DateTime? FechaLimiteEmision { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string MontoEnLetras { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? RtnCliente { get; set; }
        public bool Anulado { get; set; } = false;
        public string? MotivoAnulacion { get; set; }
    }
}
