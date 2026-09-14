using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.Clientes.DTOs
{
    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public string Rtn { get; set; } = string.Empty;
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string TipoPersona { get; set; } = "Juridica";
        public string? Rubro { get; set; }
        public string? EmailPrincipal { get; set; }
        public string? EmailSecundario { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoWhatsApp { get; set; }
        public string? Direccion { get; set; }
        public decimal CuotaMensual { get; set; }
        public int DiaCobro { get; set; }
        public string? ContrasenaSAR { get; set; }
        public bool Activo { get; set; }
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // Resumen financiero / fiscal
        public decimal TotalPagado { get; set; }
        public int PagosRealizadosCount { get; set; }
        public bool FacturasMesActualRecibidas { get; set; }
    }

    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "El RTN es obligatorio")]
        [StringLength(20, MinimumLength = 14, ErrorMessage = "El RTN debe tener al menos 14 caracteres")]
        public string Rtn { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social o nombre completo es obligatorio")]
        public string NombreRazonSocial { get; set; } = string.Empty;

        public string? NombreComercial { get; set; }

        public string TipoPersona { get; set; } = "Juridica"; // Natural o Juridica

        public string? Rubro { get; set; } // Comercio, Servicios, Restaurante, Construcción, Médico, etc.

        public string? ContrasenaSAR { get; set; }

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string? EmailPrincipal { get; set; }

        public string? EmailSecundario { get; set; }

        public string? Telefono { get; set; }

        public string? TelefonoWhatsApp { get; set; }

        public string? Direccion { get; set; }

        [Range(0, 1000000, ErrorMessage = "La cuota mensual debe ser un valor positivo")]
        public decimal CuotaMensual { get; set; } = 0.00m;

        [Range(1, 31, ErrorMessage = "El día de cobro debe estar entre 1 y 31")]
        public int DiaCobro { get; set; } = 5;

        public string? Notas { get; set; }
    }

    public class ClienteUpdateDto
    {
        public string? Rtn { get; set; }

        [Required(ErrorMessage = "La razón social o nombre completo es obligatorio")]
        public string NombreRazonSocial { get; set; } = string.Empty;

        public string? NombreComercial { get; set; }

        public string TipoPersona { get; set; } = "Juridica";

        public string? Rubro { get; set; }

        public string? ContrasenaSAR { get; set; }

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string? EmailPrincipal { get; set; }

        public string? EmailSecundario { get; set; }

        public string? Telefono { get; set; }

        public string? TelefonoWhatsApp { get; set; }

        public string? Direccion { get; set; }

        public decimal CuotaMensual { get; set; }

        public int DiaCobro { get; set; } = 5;

        public bool Activo { get; set; } = true;

        public string? Notas { get; set; }
    }

    public class ExpedienteFiscalDto
    {
        public ClienteResponseDto Cliente { get; set; } = null!;
        public List<DeclaracionAnualItemDto> DeclaracionesAnuales { get; set; } = new();
        public List<PeriodoMensualItemDto> DeclaracionesMensualesISV { get; set; } = new();
        public List<ComprobanteItemDto> ComprobantesEmitidos { get; set; } = new();
        
        // Resumen
        public int TotalDeclaracionesPresentadas { get; set; }
        public decimal TotalImpuestoLiquidadoSAR { get; set; }
        public decimal TotalHonorariosPagados { get; set; }
    }

    public class DeclaracionAnualItemDto
    {
        public int Id { get; set; }
        public int Anio { get; set; }
        public string TipoObligacion { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string FormularioSAR { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal? MontoDeclarado { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public DateTime? FechaCumplimiento { get; set; }
        public string? Observaciones { get; set; }
    }

    public class PeriodoMensualItemDto
    {
        public int Id { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string MesNombre { get; set; } = string.Empty;
        public bool FacturasRecibidas { get; set; }
        public DateTime? FechaRecepcionFacturas { get; set; }
        public int CantidadFacturasVenta { get; set; }
        public int CantidadFacturasCompra { get; set; }
        public bool LiquidadoSAR { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public decimal? MontoImpuestoISV { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class ComprobanteItemDto
    {
        public int ReciboId { get; set; }
        public string NumeroRecibo { get; set; } = string.Empty;
        public string? NumeroFiscal { get; set; }
        public string? Cai { get; set; }
        public decimal Monto { get; set; }
        public string MontoEnLetras { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string MesAplicado { get; set; } = string.Empty;
    }
}
