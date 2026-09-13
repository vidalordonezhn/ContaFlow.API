using System;

namespace ContaFlow.API.Entities
{
    public class PagoHonorario : AuditableEntity
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        public string MetodoPago { get; set; } = "Transferencia"; // Transferencia, Efectivo, Cheque, Depósito
        public string? ReferenciaBancaria { get; set; }
        public string MesAplicado { get; set; } = string.Empty; // Ej: "Enero 2026", "Febrero 2026"
        public string Estado { get; set; } = "Completado"; // Completado, Pendiente, Anulado
        public string? Observaciones { get; set; }

        // Recibo generado
        public Recibo? Recibo { get; set; }
    }
}
