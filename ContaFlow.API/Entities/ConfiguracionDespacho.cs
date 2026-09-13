using System;
using System.ComponentModel.DataAnnotations;

namespace ContaFlow.API.Entities
{
    public class ConfiguracionDespacho : AuditableEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreDespacho { get; set; } = "Despacho Contable & Fiscal";

        [MaxLength(150)]
        public string NombreContadorTitular { get; set; } = "Lic. Vidal Ordoñez";

        [MaxLength(100)]
        public string ColegiacionCAH { get; set; } = "Col. Prof. CAH # 12458";

        [MaxLength(20)]
        public string RtnDespacho { get; set; } = "08011980123456";

        [MaxLength(50)]
        public string Telefono { get; set; } = "+504 2235-0000";

        [MaxLength(50)]
        public string TelefonoWhatsApp { get; set; } = "50499887766";

        [MaxLength(120)]
        public string Email { get; set; } = "contacto@despachocontable.hn";

        [MaxLength(300)]
        public string Direccion { get; set; } = "Boulevard Morazán, Edificio Torre Alianza, Nivel 3, Tegucigalpa, M.D.C., Honduras, C.A.";

        [MaxLength(100)]
        public string Ciudad { get; set; } = "Tegucigalpa";

        [MaxLength(250)]
        public string Slogan { get; set; } = "Servicios Profesionales de Contabilidad, Auditoría y Asesoría Tributaria SAR";

        public string? LogoBase64 { get; set; }

        [MaxLength(300)]
        public string MensajePieRecibo { get; set; } = "Este documento es un comprobante digital de pago de honorarios profesionales emitido por el Despacho Contable.";

        // Banco 1 (Ej. BAC Credomatic)
        public bool Banco1Activo { get; set; } = true;
        [MaxLength(100)]
        public string Banco1Nombre { get; set; } = "BAC Credomatic";
        [MaxLength(50)]
        public string Banco1TipoCuenta { get; set; } = "Cuenta de Cheques";
        [MaxLength(50)]
        public string Banco1Numero { get; set; } = "741-234567-01";
        [MaxLength(150)]
        public string Banco1Beneficiario { get; set; } = "Despacho Contable y Fiscal";

        // Banco 2 (Ej. Banco Atlántida)
        public bool Banco2Activo { get; set; } = true;
        [MaxLength(100)]
        public string Banco2Nombre { get; set; } = "Banco Atlántida";
        [MaxLength(50)]
        public string Banco2TipoCuenta { get; set; } = "Cuenta de Ahorros";
        [MaxLength(50)]
        public string Banco2Numero { get; set; } = "110-098765-22";
        [MaxLength(150)]
        public string Banco2Beneficiario { get; set; } = "Vidal Ordoñez";

        // Banco 3 (Ej. Banco Ficohsa)
        public bool Banco3Activo { get; set; } = true;
        [MaxLength(100)]
        public string Banco3Nombre { get; set; } = "Banco Ficohsa";
        [MaxLength(50)]
        public string Banco3TipoCuenta { get; set; } = "Cuenta de Ahorros";
        [MaxLength(50)]
        public string Banco3Numero { get; set; } = "200-019283-44";
        [MaxLength(150)]
        public string Banco3Beneficiario { get; set; } = "Vidal Ordoñez";

        // Banco 4 (Ej. Banpaís / Banhcafé)
        public bool Banco4Activo { get; set; } = false;
        [MaxLength(100)]
        public string Banco4Nombre { get; set; } = "Banco del País (Banpaís)";
        [MaxLength(50)]
        public string Banco4TipoCuenta { get; set; } = "Cuenta de Ahorros";
        [MaxLength(50)]
        public string Banco4Numero { get; set; } = "01-502-000123-9";
        [MaxLength(150)]
        public string Banco4Beneficiario { get; set; } = "Despacho Contable";
    }
}
