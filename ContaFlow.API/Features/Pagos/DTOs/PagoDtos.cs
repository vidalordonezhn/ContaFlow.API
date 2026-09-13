using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.Pagos.DTOs
{
    public class PagoResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteRtn { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; } = "Transferencia";
        public string? ReferenciaBancaria { get; set; }
        public string MesAplicado { get; set; } = string.Empty;
        public string Estado { get; set; } = "Completado";
        public string? Observaciones { get; set; }
        public int? ReciboId { get; set; }
        public string? NumeroRecibo { get; set; }
    }

    public class PagoCreateDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public string MetodoPago { get; set; } = "Transferencia"; // Transferencia, Efectivo, Cheque, Depósito

        public string? ReferenciaBancaria { get; set; }

        [Required(ErrorMessage = "El mes aplicado es obligatorio (ej: 'Septiembre 2026')")]
        public string MesAplicado { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public bool GenerarRecibo { get; set; } = true;
    }
}
