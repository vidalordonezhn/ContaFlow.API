using System;

namespace ContaFlow.API.Features.Recordatorios.DTOs
{
    public class RecordatorioResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteRtn { get; set; } = string.Empty;
        public string? TelefonoWhatsApp { get; set; }
        public string? Email { get; set; }
        public decimal? CuotaMensual { get; set; }
        
        public string Tipo { get; set; } = "SAR";
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = "WhatsApp";
        public string Estado { get; set; } = "Pendiente";
        
        public DateTime? FechaEnvio { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class RecordatorioCreateDto
    {
        public int ClienteId { get; set; }
        public string Tipo { get; set; } = "SAR";
        public string? Titulo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Canal { get; set; } = "WhatsApp";
        public string? Estado { get; set; } = "Pendiente";
        public string? TelefonoDestino { get; set; }
        public string? EmailDestino { get; set; }
    }

    public class RecordatorioUpdateDto
    {
        public string? Titulo { get; set; }
        public string? Mensaje { get; set; }
        public string? Estado { get; set; }
        public string? Canal { get; set; }
    }
}
