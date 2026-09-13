using System;

namespace ContaFlow.API.Entities
{
    public abstract class AuditableEntity
    {
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? CreadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
    }
}
