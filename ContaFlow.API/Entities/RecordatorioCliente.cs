using System;

namespace ContaFlow.API.Entities
{
    public class RecordatorioCliente : AuditableEntity
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        
        public string Tipo { get; set; } = "SAR"; // SAR, Cobro, Declaracion, Personalizado
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = "WhatsApp"; // WhatsApp, Email, Ambos
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Enviado, Respondido, Omitido
        
        public DateTime? FechaEnvio { get; set; }
        public string? TelefonoDestino { get; set; }
        public string? EmailDestino { get; set; }
    }
}
