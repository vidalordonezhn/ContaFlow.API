using System;

namespace ContaFlow.API.Entities
{
    public class Usuario : AuditableEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Contador"; // Admin, Contador, Asistente
        public bool Activo { get; set; } = true;
        public DateTime? UltimoAcceso { get; set; }
    }
}
