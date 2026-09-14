using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Features.Recibos.DTOs
{
    public class ReciboItemDto
    {
        public string Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; } = 1;
        public decimal Precio { get; set; } = 0;
        public decimal Total { get; set; } = 0;
    }

    public class ReciboResponseDto
    {
        public int Id { get; set; }
        public int? PagoHonorarioId { get; set; }
        public int? ClienteId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string RtnCliente { get; set; } = string.Empty;
        public string TipoComprobante { get; set; } = "SinCAI"; // "SinCAI" o "ConCAI"
        public string NumeroRecibo { get; set; } = string.Empty;
        public string? NumeroFiscal { get; set; }
        public string? Cai { get; set; }
        public string? RangoAutorizado { get; set; }
        public DateTime? FechaLimiteEmision { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Monto { get; set; }
        public string MontoEnLetras { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public bool Anulado { get; set; }
        public string? MotivoAnulacion { get; set; }
        public List<ReciboItemDto> Items { get; set; } = new();
    }

    public class ReciboCreateDto
    {
        public int? ClienteId { get; set; }
        public string? NombreCliente { get; set; }
        public string? RtnCliente { get; set; }
        public string TipoComprobante { get; set; } = "SinCAI"; // "SinCAI" o "ConCAI"
        public string? NumeroRecibo { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
        public string? Concepto { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; } = "Transferencia";
        public string? Observaciones { get; set; }
        public bool RegistrarComoPago { get; set; } = false;
        public string? MesAplicado { get; set; }
        public List<ReciboItemDto> Items { get; set; } = new();
    }
}
