using System.Threading.Tasks;
using BCrypt.Net;
using ContaFlow.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ContaFlowDbContext context)
        {
            // Asegurar que la tabla configuracion_despacho exista
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS configuracion_despacho (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""NombreDespacho"" VARCHAR(200) NOT NULL,
                    ""NombreContadorTitular"" VARCHAR(150),
                    ""ColegiacionCAH"" VARCHAR(100),
                    ""RtnDespacho"" VARCHAR(20),
                    ""Telefono"" VARCHAR(50),
                    ""TelefonoWhatsApp"" VARCHAR(50),
                    ""Email"" VARCHAR(120),
                    ""Direccion"" VARCHAR(300),
                    ""Ciudad"" VARCHAR(100),
                    ""Slogan"" VARCHAR(250),
                    ""LogoBase64"" TEXT,
                    ""MensajePieRecibo"" VARCHAR(300),
                    ""Banco1Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Banco1Nombre"" VARCHAR(100),
                    ""Banco1TipoCuenta"" VARCHAR(50),
                    ""Banco1Numero"" VARCHAR(50),
                    ""Banco1Beneficiario"" VARCHAR(150),
                    ""Banco2Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Banco2Nombre"" VARCHAR(100),
                    ""Banco2TipoCuenta"" VARCHAR(50),
                    ""Banco2Numero"" VARCHAR(50),
                    ""Banco2Beneficiario"" VARCHAR(150),
                    ""Banco3Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Banco3Nombre"" VARCHAR(100),
                    ""Banco3TipoCuenta"" VARCHAR(50),
                    ""Banco3Numero"" VARCHAR(50),
                    ""Banco3Beneficiario"" VARCHAR(150),
                    ""Banco4Activo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""Banco4Nombre"" VARCHAR(100),
                    ""Banco4TipoCuenta"" VARCHAR(50),
                    ""Banco4Numero"" VARCHAR(50),
                    ""Banco4Beneficiario"" VARCHAR(150),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );
            ");

            // Asegurar que la tabla autorizaciones_cai exista y tenga columnas fiscales
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS autorizaciones_cai (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Cai"" VARCHAR(45) NOT NULL,
                    ""TipoDocumento"" VARCHAR(100) NOT NULL DEFAULT 'Factura',
                    ""Establecimiento"" VARCHAR(3) NOT NULL DEFAULT '000',
                    ""PuntoEmision"" VARCHAR(3) NOT NULL DEFAULT '001',
                    ""TipoDocCodigo"" VARCHAR(2) NOT NULL DEFAULT '01',
                    ""RangoInicial"" INT NOT NULL DEFAULT 1,
                    ""RangoFinal"" INT NOT NULL DEFAULT 500,
                    ""CorrelativoActual"" INT NOT NULL DEFAULT 1,
                    ""FechaLimiteEmision"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    ""FechaRecepcionSAR"" TIMESTAMP WITH TIME ZONE,
                    ""Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Observaciones"" VARCHAR(250),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""NumeroFiscal"" VARCHAR(60);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""AutorizacionCAIId"" INT;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""Cai"" VARCHAR(50);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""RangoAutorizado"" VARCHAR(100);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""FechaLimiteEmision"" TIMESTAMP WITH TIME ZONE;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""ClienteId"" INT;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""NombreCliente"" VARCHAR(200);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""RtnCliente"" VARCHAR(50);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""TipoComprobante"" VARCHAR(50) DEFAULT 'SinCAI';
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""MetodoPago"" VARCHAR(50);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""Anulado"" BOOLEAN NOT NULL DEFAULT FALSE;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""MotivoAnulacion"" VARCHAR(300);
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""ItemsJson"" TEXT;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""Subtotal"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""Impuesto"" NUMERIC(18, 2) NOT NULL DEFAULT 0;

                CREATE TABLE IF NOT EXISTS seguimientos_fiscales_anuales (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ClienteId"" INT NOT NULL REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Anio"" INT NOT NULL,
                    ""TipoObligacion"" VARCHAR(50) NOT NULL,
                    ""Estado"" VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
                    ""MontoDeclarado"" NUMERIC(18, 2),
                    ""NumeroDeclaracionSAR"" VARCHAR(100),
                    ""FechaCumplimiento"" TIMESTAMP WITH TIME ZONE,
                    ""Observaciones"" VARCHAR(300),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""ContrasenaSAR"" VARCHAR(100);

                CREATE TABLE IF NOT EXISTS catalogo_rubros (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nombre"" VARCHAR(150) NOT NULL UNIQUE,
                    ""Descripcion"" VARCHAR(300),
                    ""Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                CREATE TABLE IF NOT EXISTS libros_detalle_items (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PeriodoFiscalId"" INT NOT NULL REFERENCES periodos_fiscales_sar(""Id"") ON DELETE CASCADE,
                    ""Correlativo"" INT NOT NULL DEFAULT 1,
                    ""Fecha"" TIMESTAMP WITH TIME ZONE,
                    ""Proveedor"" VARCHAR(200),
                    ""ComprasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ComprasGravadas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvCompras15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""FacturaNumero"" VARCHAR(50),
                    ""VentasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""VentasGravadas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvVentas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Notas"" VARCHAR(300),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                CREATE TABLE IF NOT EXISTS libros_ventas_items (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PeriodoFiscalId"" INT NOT NULL REFERENCES periodos_fiscales_sar(""Id"") ON DELETE CASCADE,
                    ""ClienteId"" INT REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Correlativo"" INT NOT NULL DEFAULT 1,
                    ""Fecha"" TIMESTAMP WITH TIME ZONE,
                    ""Factura"" VARCHAR(60),
                    ""Exonerado"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Exento"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Gravado15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Gravado18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Isv15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Isv18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Total"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Notas"" VARCHAR(300),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                CREATE TABLE IF NOT EXISTS libros_compras_items (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PeriodoFiscalId"" INT NOT NULL REFERENCES periodos_fiscales_sar(""Id"") ON DELETE CASCADE,
                    ""ClienteId"" INT REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Correlativo"" INT NOT NULL DEFAULT 1,
                    ""Fecha"" TIMESTAMP WITH TIME ZONE,
                    ""Factura"" VARCHAR(60),
                    ""Proveedor"" VARCHAR(200),
                    ""Exonerado"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Exento"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Gravado15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Gravado18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Isv15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Isv18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Total"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Notas"" VARCHAR(300),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Columnas para Libros ISV (SAR-210) & Liquidación
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""VentasGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""VentasGravadas18"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""VentasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""IsvDebito15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""IsvDebito18"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""TotalDebitoFiscal"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ComprasGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ComprasGravadas18"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ComprasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ImportacionesGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""IsvCredito15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""IsvCredito18"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""TotalCreditoFiscal"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""SaldoAFavorPeriodoAnterior"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""RetencionesISVRecibidas"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""Retenciones15"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""Retenciones18"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ServiciosProfesionales"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""ImpuestoDeterminadoPagar"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""SaldoAFavorContribuyente"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
            ");

            // Sembrar catálogo de rubros iniciales si no existen
            if (!await context.Rubros.AnyAsync())
            {
                var rubrosDefault = new[]
                {
                    new Rubro { Nombre = "Comercio General", Descripcion = "Compra y venta de mercaderías en general" },
                    new Rubro { Nombre = "Servicios Profesionales", Descripcion = "Asesorías, contabilidad, legal, auditoría" },
                    new Rubro { Nombre = "Restaurante / Alimentos", Descripcion = "Gastronomía, comidas preparadas, cafeterías" },
                    new Rubro { Nombre = "Construcción e Ingeniería", Descripcion = "Edificaciones, obras civiles, contratistas" },
                    new Rubro { Nombre = "Salud y Farmacia", Descripcion = "Clínicas, médicos, odontología, farmacias" },
                    new Rubro { Nombre = "Transporte y Logística", Descripcion = "Carga pesada, encomiendas, transporte de pasajeros" },
                    new Rubro { Nombre = "Tecnología e Informática", Descripcion = "Desarrollo de software, soporte, telecomunicaciones" },
                    new Rubro { Nombre = "Bienes Raíces", Descripcion = "Arrendamientos, compra-venta inmobiliaria" },
                    new Rubro { Nombre = "Taller / Automotriz", Descripcion = "Mecánica, repuestos, llanteras" },
                    new Rubro { Nombre = "Educación y Capacitación", Descripcion = "Institutos, escuelas, tutorías" },
                    new Rubro { Nombre = "Agricultura y Ganadería", Descripcion = "Cultivos, producción agropecuaria" },
                    new Rubro { Nombre = "Otro Rubro", Descripcion = "Otras actividades económicas" }
                };

                await context.Rubros.AddRangeAsync(rubrosDefault);
                await context.SaveChangesAsync();
            }

            // Si no hay configuración registrada, creamos la configuración inicial del despacho
            if (!await context.ConfiguracionDespacho.AnyAsync())
            {
                var config = new ConfiguracionDespacho
                {
                    NombreDespacho = "DESPACHO CONTABLE & FISCAL ORDOÑEZ",
                    NombreContadorTitular = "Lic. Vidal Ordoñez",
                    ColegiacionCAH = "Col. Prof. CAH # 12458",
                    RtnDespacho = "08011980123456",
                    Telefono = "+504 2235-0000",
                    TelefonoWhatsApp = "50499887766",
                    Email = "contacto@despachocontable.hn",
                    Direccion = "Boulevard Morazán, Edificio Torre Alianza, Nivel 3, Tegucigalpa, M.D.C., Honduras, C.A.",
                    Ciudad = "Tegucigalpa",
                    Slogan = "Servicios Profesionales de Contabilidad, Auditoría y Asesoría Tributaria SAR",
                    MensajePieRecibo = "Este documento es un comprobante digital de pago de honorarios profesionales emitido conforme a las normas contables vigentes.",
                    Banco1Activo = true,
                    Banco1Nombre = "BAC Credomatic",
                    Banco1TipoCuenta = "Cuenta de Cheques",
                    Banco1Numero = "741-234567-01",
                    Banco1Beneficiario = "Despacho Contable y Fiscal",
                    Banco2Activo = true,
                    Banco2Nombre = "Banco Atlántida",
                    Banco2TipoCuenta = "Cuenta de Ahorros",
                    Banco2Numero = "110-098765-22",
                    Banco2Beneficiario = "Vidal Ordoñez",
                    Banco3Activo = true,
                    Banco3Nombre = "Banco Ficohsa",
                    Banco3TipoCuenta = "Cuenta de Ahorros",
                    Banco3Numero = "200-019283-44",
                    Banco3Beneficiario = "Vidal Ordoñez",
                    Banco4Activo = false,
                    Banco4Nombre = "Banco del País (Banpaís)",
                    Banco4TipoCuenta = "Cuenta de Ahorros",
                    Banco4Numero = "01-502-000123-9",
                    Banco4Beneficiario = "Despacho Contable",
                    FechaCreacion = System.DateTime.UtcNow,
                    CreadoPor = "sistema"
                };

                await context.ConfiguracionDespacho.AddAsync(config);
                await context.SaveChangesAsync();
            }

            // Si no hay CAI registrado, creamos el lote de CAI inicial autorizado por SAR
            if (!await context.AutorizacionesCAI.AnyAsync())
            {
                var caiInicial = new AutorizacionCAI
                {
                    Cai = "3C4F81-912A34-034389-112233-445566-77",
                    TipoDocumento = "Recibo por Honorarios Profesionales",
                    Establecimiento = "000",
                    PuntoEmision = "001",
                    TipoDocCodigo = "01",
                    RangoInicial = 1,
                    RangoFinal = 500,
                    CorrelativoActual = 1,
                    FechaLimiteEmision = System.DateTime.UtcNow.AddMonths(12),
                    FechaRecepcionSAR = System.DateTime.UtcNow.AddDays(-15),
                    Activo = true,
                    Observaciones = "Autorización SAR vigente para emisión de recibos y facturas de honorarios.",
                    FechaCreacion = System.DateTime.UtcNow,
                    CreadoPor = "sistema"
                };

                await context.AutorizacionesCAI.AddAsync(caiInicial);
                await context.SaveChangesAsync();
            }

            // Si no hay usuarios registrados, creamos el administrador y contador inicial
            if (!await context.Usuarios.AnyAsync())
            {
                var admin = new Usuario
                {
                    Username = "admin",
                    Nombre = "Administrador del Despacho",
                    Email = "admin@contaflow.com",
                    Telefono = "+504 9999-0000",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                    Rol = "Admin",
                    Activo = true
                };

                var contadorPrincipal = new Usuario
                {
                    Username = "contador",
                    Nombre = "Contador General",
                    Email = "contador@contaflow.com",
                    Telefono = "+504 9888-1111",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contador123*"),
                    Rol = "Contador",
                    Activo = true
                };

                await context.Usuarios.AddRangeAsync(admin, contadorPrincipal);
                await context.SaveChangesAsync();
            }

            // Si no hay clientes registrados, creamos clientes de prueba realistas para Honduras
            if (!await context.Clientes.AnyAsync())
            {
                var hoy = System.DateTime.UtcNow;

                var cliente1 = new Cliente
                {
                    Rtn = "08011990145238",
                    NombreRazonSocial = "Distribuidora Comercial del Valle S. de R.L.",
                    NombreComercial = "Super Valle",
                    TipoPersona = "Juridica",
                    Rubro = "Comercio General",
                    ContrasenaSAR = "ValleSAR2026*",
                    CuotaMensual = 3500.00m,
                    DiaCobro = 5,
                    Telefono = "+504 2235-8899",
                    TelefonoWhatsApp = "50499887766",
                    EmailPrincipal = "contabilidad@supervalle.hn",
                    Direccion = "Boulevard Morazán, Edificio Torre Alianza, Piso 4, Tegucigalpa, M.D.C.",
                    Notas = "Declarante ISV general mensual y retenciones 1% y 12.5%.",
                    Activo = true,
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                var cliente2 = new Cliente
                {
                    Rtn = "08011985123456",
                    NombreRazonSocial = "Dra. Elena Sofía Morales Rivera",
                    NombreComercial = "Clínica Dental Morales",
                    TipoPersona = "Natural",
                    Rubro = "Salud y Farmacia",
                    ContrasenaSAR = "Dental2026*",
                    CuotaMensual = 2000.00m,
                    DiaCobro = 10,
                    Telefono = "+504 2238-1122",
                    TelefonoWhatsApp = "50498765432",
                    EmailPrincipal = "dra.morales@clinicadental.hn",
                    Direccion = "Colonia Palmira, Avenida República de Chile, Tegucigalpa",
                    Notas = "Profesional independiente. Pagos a cuenta y retención 10%.",
                    Activo = true,
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                var cliente3 = new Cliente
                {
                    Rtn = "05011978998877",
                    NombreRazonSocial = "Inversiones y Construcciones Lempira S.A.",
                    NombreComercial = "Constructora Lempira",
                    TipoPersona = "Juridica",
                    Rubro = "Construcción e Ingeniería",
                    ContrasenaSAR = "LempiraSAR2026*",
                    CuotaMensual = 5000.00m,
                    DiaCobro = 1,
                    Telefono = "+504 2550-3344",
                    TelefonoWhatsApp = "50433221100",
                    EmailPrincipal = "administracion@constructoralempira.hn",
                    Direccion = "Barrio Los Andes, 7ma Calle, San Pedro Sula, Cortés",
                    Notas = "Gran contribuyente regional. Emisión de órdenes de compra y contratos de obra.",
                    Activo = true,
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                await context.Clientes.AddRangeAsync(cliente1, cliente2, cliente3);
                await context.SaveChangesAsync();

                // Crear periodo fiscal SAR actual (Septiembre 2026) para cada uno
                var sar1 = new PeriodoFiscalSAR
                {
                    ClienteId = cliente1.Id,
                    Mes = hoy.Month,
                    Anio = hoy.Year,
                    FacturasRecibidas = false,
                    LiquidadoSAR = false,
                    Estado = "Pendiente",
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                var sar2 = new PeriodoFiscalSAR
                {
                    ClienteId = cliente2.Id,
                    Mes = hoy.Month,
                    Anio = hoy.Year,
                    FacturasRecibidas = true,
                    FechaRecepcionFacturas = hoy.AddDays(-2),
                    CantidadFacturasVenta = 18,
                    CantidadFacturasCompra = 24,
                    NotasDocumentos = "Facturas físicas y electrónicas recibidas en orden.",
                    LiquidadoSAR = false,
                    Estado = "EnProceso",
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                var sar3 = new PeriodoFiscalSAR
                {
                    ClienteId = cliente3.Id,
                    Mes = hoy.Month,
                    Anio = hoy.Year,
                    FacturasRecibidas = true,
                    FechaRecepcionFacturas = hoy.AddDays(-5),
                    CantidadFacturasVenta = 42,
                    CantidadFacturasCompra = 87,
                    LiquidadoSAR = true,
                    FechaLiquidacion = hoy.AddDays(-1),
                    NumeroDeclaracionSAR = "SAR-DEC-202609-00941",
                    MontoImpuestoISV = 14250.00m,
                    Estado = "Declarado",
                    FechaCreacion = hoy,
                    CreadoPor = "admin"
                };

                await context.PeriodosFiscalesSAR.AddRangeAsync(sar1, sar2, sar3);
                await context.SaveChangesAsync();
            }
        }
    }
}
