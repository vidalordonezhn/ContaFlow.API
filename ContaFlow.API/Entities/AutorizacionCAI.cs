using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Entities
{
    public class AutorizacionCAI : AuditableEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(45)]
        public string Cai { get; set; } = string.Empty; // Ej: 3C4F81-912A34-034389-112233-445566-77

        [Required]
        [MaxLength(100)]
        public string TipoDocumento { get; set; } = "Factura"; // Factura o Recibo por Honorarios Profesionales

        [Required]
        [MaxLength(3)]
        public string Establecimiento { get; set; } = "000";

        [Required]
        [MaxLength(3)]
        public string PuntoEmision { get; set; } = "001";

        [Required]
        [MaxLength(2)]
        public string TipoDocCodigo { get; set; } = "01";

        public int RangoInicial { get; set; } = 1;
        public int RangoFinal { get; set; } = 500;
        public int CorrelativoActual { get; set; } = 1;

        public DateTime FechaLimiteEmision { get; set; } = DateTime.UtcNow.AddYears(1);
        public DateTime? FechaRecepcionSAR { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        // Métodos auxiliares de formato SAR
        public string FormatearNumero(int secuencia)
        {
            return $"{Establecimiento}-{PuntoEmision}-{TipoDocCodigo}-{secuencia:D8}";
        }

        public string RangoInicialFormateado => FormatearNumero(RangoInicial);
        public string RangoFinalFormateado => FormatearNumero(RangoFinal);
        public string SiguienteNumeroFormateado => FormatearNumero(CorrelativoActual);

        public int TotalAutorizados => Math.Max(0, RangoFinal - RangoInicial + 1);
        public int TotalConsumidos => Math.Max(0, CorrelativoActual - RangoInicial);
        public int TotalRestantes => Math.Max(0, RangoFinal - CorrelativoActual + 1);
        public double PorcentajeConsumido => TotalAutorizados > 0 ? Math.Round((double)TotalConsumidos / TotalAutorizados * 100, 1) : 0;
        public bool EstaAgotado => CorrelativoActual > RangoFinal;
        public bool EstaVencido => DateTime.UtcNow > FechaLimiteEmision;
        public int DiasRestantes => Math.Max(0, (int)(FechaLimiteEmision - DateTime.UtcNow).TotalDays);
    }
}
