using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.SARControl.DTOs
{
    public class PeriodoSARResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteRtn { get; set; } = string.Empty;
        public string? ClienteRubro { get; set; }
        public string? ClienteWhatsApp { get; set; }
        public string? ClienteEmail { get; set; }

        public int Mes { get; set; }
        public int Anio { get; set; }
        public string MesNombre { get; set; } = string.Empty;

        // Semáforo y recepción
        public bool FacturasRecibidas { get; set; }
        public DateTime? FechaRecepcionFacturas { get; set; }
        public int CantidadFacturasVenta { get; set; }
        public int CantidadFacturasCompra { get; set; }
        public string? NotasDocumentos { get; set; }

        // Liquidación / Declaración
        public bool LiquidadoSAR { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
        public decimal? MontoImpuestoISV { get; set; }
        public decimal? MontoRetenciones { get; set; }
        public string? NumeroDeclaracionSAR { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, EnProceso, Liquidado, Declarado
        public string NivelSemaforo { get; set; } = "Rojo"; // Verde, Amarillo, Rojo
    }

    public class MarcarRecepcionDto
    {
        public bool FacturasRecibidas { get; set; } = true;
        public int CantidadFacturasVenta { get; set; } = 0;
        public int CantidadFacturasCompra { get; set; } = 0;
        public string? NotasDocumentos { get; set; }
    }

    public class RegistrarLiquidacionSARDto
    {
        [Required(ErrorMessage = "El número de declaración o comprobante SAR es requerido")]
        public string NumeroDeclaracionSAR { get; set; } = string.Empty;

        public decimal MontoImpuestoISV { get; set; } = 0.00m;
        public decimal MontoRetenciones { get; set; } = 0.00m;
        public string? Notas { get; set; }
    }

    public class SARResumenMensualDto
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int TotalClientesActivos { get; set; }
        public int FacturasRecibidasCount { get; set; }
        public int FacturasPendientesCount { get; set; }
        public int LiquidadosSARCount { get; set; }
        public int DiasRestantesParaDia10 { get; set; }
        public bool AlertaDia10Proximo { get; set; }
    }
}
