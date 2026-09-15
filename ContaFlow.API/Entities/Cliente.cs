using System;
using System.Collections.Generic;

namespace ContaFlow.API.Entities
{
    public class Cliente : AuditableEntity
    {
        public int Id { get; set; }
        public string Rtn { get; set; } = string.Empty; // RTN Fiscal Honduras (14 dígitos)
        public string NombreRazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string TipoPersona { get; set; } = "Juridica"; // Natural, Juridica
        public string? Rubro { get; set; } // Comercio, Servicios, Restaurante, Construcción, etc.
        public string? EmailPrincipal { get; set; }
        public string? EmailSecundario { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoWhatsApp { get; set; }
        public string? Direccion { get; set; }
        
        // Identificación & Ubicación
        public string? Dni { get; set; } // DNI / Identidad (13 dígitos)
        public string? RepresentanteLegalNombre { get; set; }
        public string? RepresentanteLegalRtn { get; set; }
        public int? DepartamentoId { get; set; }
        public string? DepartamentoNombre { get; set; }
        public int? MunicipioId { get; set; }
        public string? MunicipioNombre { get; set; }

        // Configuración de Honorarios
        public decimal CuotaMensual { get; set; } = 0.00m;
        public int DiaCobro { get; set; } = 5; // Día del mes sugerido para cobro
        public string? ContrasenaSAR { get; set; } // Contraseña Portal SAR Oficina Virtual
        public bool Activo { get; set; } = true;
        public string? Notas { get; set; }

        // Relaciones
        public ICollection<PagoHonorario> Pagos { get; set; } = new List<PagoHonorario>();
        public ICollection<PeriodoFiscalSAR> PeriodosFiscales { get; set; } = new List<PeriodoFiscalSAR>();
        public ICollection<LibroVentaDetalleItem> LibrosVentas { get; set; } = new List<LibroVentaDetalleItem>();
        public ICollection<LibroCompraDetalleItem> LibrosCompras { get; set; } = new List<LibroCompraDetalleItem>();
    }
}
