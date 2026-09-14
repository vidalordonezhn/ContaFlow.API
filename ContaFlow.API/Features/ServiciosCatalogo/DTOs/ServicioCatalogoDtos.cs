using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.ServiciosCatalogo.DTOs
{
    public class ServicioCatalogoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? DescripcionDefault { get; set; }
        public decimal PrecioDefault { get; set; }
        public string? Categoria { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class ServicioCatalogoCreateDto
    {
        [Required(ErrorMessage = "El nombre del producto o servicio es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? DescripcionDefault { get; set; }

        [Range(0, 1000000, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PrecioDefault { get; set; }

        public string? Categoria { get; set; } = "General";
    }

    public class ServicioCatalogoUpdateDto
    {
        [Required(ErrorMessage = "El nombre del producto o servicio es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? DescripcionDefault { get; set; }

        [Range(0, 1000000, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PrecioDefault { get; set; }

        public string? Categoria { get; set; }
        public bool Activo { get; set; } = true;
    }
}
