using System;

namespace ContaFlow.API.Entities
{
    public class ServicioCatalogo : AuditableEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? DescripcionDefault { get; set; }
        public decimal PrecioDefault { get; set; }
        public string? Categoria { get; set; } = "General";
        public bool Activo { get; set; } = true;
    }
}
