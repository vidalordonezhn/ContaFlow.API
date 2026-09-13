using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.Usuarios.DTOs
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string? Email { get; set; }

        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "Contador"; // Admin, Contador, Asistente
    }

    public class UsuarioUpdateDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string? Email { get; set; }

        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "Contador";

        public bool Activo { get; set; } = true;
    }

    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
