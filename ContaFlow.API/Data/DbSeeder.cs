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
            // ── 1. Crear todas las tablas en PostgreSQL si no existen ────────────────
            await context.Database.ExecuteSqlRawAsync(@"
                -- Departamentos
                CREATE TABLE IF NOT EXISTS departamentos (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Codigo"" VARCHAR(10) NOT NULL UNIQUE,
                    ""Nombre"" VARCHAR(100) NOT NULL,
                    ""Cabecera"" VARCHAR(100)
                );

                -- Municipios
                CREATE TABLE IF NOT EXISTS municipios (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""DepartamentoId"" INT NOT NULL REFERENCES departamentos(""Id"") ON DELETE CASCADE,
                    ""Codigo"" VARCHAR(10) NOT NULL UNIQUE,
                    ""Nombre"" VARCHAR(150) NOT NULL
                );

                -- Catálogo de Rubros
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

                -- Usuarios
                CREATE TABLE IF NOT EXISTS usuarios (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Username"" VARCHAR(100) NOT NULL UNIQUE,
                    ""Nombre"" VARCHAR(150) NOT NULL,
                    ""Email"" VARCHAR(150),
                    ""Telefono"" VARCHAR(50),
                    ""PasswordHash"" VARCHAR(300) NOT NULL,
                    ""Rol"" VARCHAR(50) NOT NULL DEFAULT 'Contador',
                    ""Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""UltimoAcceso"" TIMESTAMP WITH TIME ZONE,
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Clientes
                CREATE TABLE IF NOT EXISTS clientes (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Rtn"" VARCHAR(20) NOT NULL UNIQUE,
                    ""NombreRazonSocial"" VARCHAR(200) NOT NULL,
                    ""NombreComercial"" VARCHAR(200),
                    ""TipoPersona"" VARCHAR(50) NOT NULL DEFAULT 'Natural',
                    ""Rubro"" VARCHAR(100),
                    ""ContrasenaSAR"" VARCHAR(100),
                    ""Dni"" VARCHAR(20),
                    ""RepresentanteLegalNombre"" VARCHAR(150),
                    ""RepresentanteLegalRtn"" VARCHAR(20),
                    ""DepartamentoId"" INT,
                    ""DepartamentoNombre"" VARCHAR(100),
                    ""MunicipioId"" INT,
                    ""MunicipioNombre"" VARCHAR(100),
                    ""CuotaMensual"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""DiaCobro"" INT NOT NULL DEFAULT 1,
                    ""Telefono"" VARCHAR(50),
                    ""TelefonoWhatsApp"" VARCHAR(50),
                    ""EmailPrincipal"" VARCHAR(120),
                    ""EmailSecundario"" VARCHAR(120),
                    ""Direccion"" VARCHAR(300),
                    ""Notas"" VARCHAR(500),
                    ""Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Configuración Despacho
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

                -- Autorizaciones CAI
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

                -- Pagos de Honorarios
                CREATE TABLE IF NOT EXISTS pagos_honorarios (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ClienteId"" INT NOT NULL REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Monto"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""FechaPago"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""MetodoPago"" VARCHAR(50) NOT NULL DEFAULT 'Transferencia',
                    ""ReferenciaBancaria"" VARCHAR(100),
                    ""MesAplicado"" VARCHAR(50) NOT NULL DEFAULT '',
                    ""Estado"" VARCHAR(30) NOT NULL DEFAULT 'Completado',
                    ""Observaciones"" VARCHAR(300),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Recibos
                CREATE TABLE IF NOT EXISTS recibos (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PagoHonorarioId"" INT REFERENCES pagos_honorarios(""Id"") ON DELETE SET NULL,
                    ""TipoComprobante"" VARCHAR(50) DEFAULT 'SinCAI',
                    ""NumeroRecibo"" VARCHAR(50) NOT NULL UNIQUE,
                    ""NumeroFiscal"" VARCHAR(60),
                    ""AutorizacionCAIId"" INT REFERENCES autorizaciones_cai(""Id"") ON DELETE SET NULL,
                    ""Cai"" VARCHAR(50),
                    ""RangoAutorizado"" VARCHAR(100),
                    ""FechaLimiteEmision"" TIMESTAMP WITH TIME ZONE,
                    ""FechaEmision"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""Concepto"" VARCHAR(300) NOT NULL DEFAULT '',
                    ""Subtotal"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Impuesto"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Monto"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""MontoEnLetras"" VARCHAR(300),
                    ""ClienteId"" INT REFERENCES clientes(""Id"") ON DELETE SET NULL,
                    ""NombreCliente"" VARCHAR(200),
                    ""RtnCliente"" VARCHAR(50),
                    ""MetodoPago"" VARCHAR(50),
                    ""Anulado"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""MotivoAnulacion"" VARCHAR(300),
                    ""ItemsJson"" TEXT,
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Periodos Fiscales SAR
                CREATE TABLE IF NOT EXISTS periodos_fiscales_sar (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ClienteId"" INT NOT NULL REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Mes"" INT NOT NULL,
                    ""Anio"" INT NOT NULL,
                    ""FacturasRecibidas"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""FechaRecepcionFacturas"" TIMESTAMP WITH TIME ZONE,
                    ""CantidadFacturasVenta"" INT NOT NULL DEFAULT 0,
                    ""CantidadFacturasCompra"" INT NOT NULL DEFAULT 0,
                    ""NotasDocumentos"" VARCHAR(300),
                    ""VentasGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""VentasGravadas18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""VentasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvDebito15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvDebito18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""TotalDebitoFiscal"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ComprasGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ComprasGravadas18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ComprasExentas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ImportacionesGravadas15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvCredito15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""IsvCredito18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""TotalCreditoFiscal"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""SaldoAFavorPeriodoAnterior"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""RetencionesISVRecibidas"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Retenciones15"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Retenciones18"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ServiciosProfesionales"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""ImpuestoDeterminadoPagar"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""SaldoAFavorContribuyente"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""LiquidadoSAR"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""FechaLiquidacion"" TIMESTAMP WITH TIME ZONE,
                    ""NumeroDeclaracionSAR"" VARCHAR(100),
                    ""MontoImpuestoISV"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""MontoRetenciones"" NUMERIC(18, 2),
                    ""Estado"" VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Asegurar columnas críticas en tablas existentes
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""MontoRetenciones"" NUMERIC(18, 2);
                ALTER TABLE periodos_fiscales_sar ADD COLUMN IF NOT EXISTS ""MontoImpuestoISV"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""Monto"" NUMERIC(18, 2) NOT NULL DEFAULT 0;
                ALTER TABLE recibos ADD COLUMN IF NOT EXISTS ""PagoHonorarioId"" INT;
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""Dni"" VARCHAR(20);
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""RepresentanteLegalNombre"" VARCHAR(150);
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""RepresentanteLegalRtn"" VARCHAR(20);
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""DepartamentoId"" INT;
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""DepartamentoNombre"" VARCHAR(100);
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""MunicipioId"" INT;
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""MunicipioNombre"" VARCHAR(100);
                ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ""ContrasenaSAR"" VARCHAR(100);

                -- Seguimientos Fiscales Anuales
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

                -- Servicios Catálogo
                CREATE TABLE IF NOT EXISTS servicios_catalogo (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nombre"" VARCHAR(200) NOT NULL,
                    ""DescripcionDefault"" VARCHAR(300),
                    ""PrecioDefault"" NUMERIC(18, 2) NOT NULL DEFAULT 0,
                    ""Categoria"" VARCHAR(100) DEFAULT 'General',
                    ""Activo"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Recordatorios Clientes
                CREATE TABLE IF NOT EXISTS recordatorios_clientes (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ClienteId"" INT NOT NULL REFERENCES clientes(""Id"") ON DELETE CASCADE,
                    ""Tipo"" VARCHAR(50) NOT NULL DEFAULT 'SAR',
                    ""Titulo"" VARCHAR(150),
                    ""Mensaje"" TEXT NOT NULL,
                    ""Canal"" VARCHAR(50) NOT NULL DEFAULT 'WhatsApp',
                    ""Estado"" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
                    ""FechaEnvio"" TIMESTAMP WITH TIME ZONE,
                    ""TelefonoDestino"" VARCHAR(50),
                    ""EmailDestino"" VARCHAR(120),
                    ""FechaCreacion"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreadoPor"" VARCHAR(100),
                    ""FechaModificacion"" TIMESTAMP WITH TIME ZONE,
                    ""ModificadoPor"" VARCHAR(100)
                );

                -- Libros Detalle Items
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

                -- Libros Ventas Items
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

                -- Libros Compras Items
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
            ");

            // Sembrar catálogo de productos / servicios iniciales si no existen
            if (!await context.ServiciosCatalogo.AnyAsync())
            {
                var serviciosDefault = new[]
                {
                    new ServicioCatalogo { Nombre = "Talonario de Facturas", DescripcionDefault = "Talonario de facturas fiscales de 3 copias", PrecioDefault = 350, Categoria = "Talonarios", Activo = true },
                    new ServicioCatalogo { Nombre = "Constancia Electrónica", DescripcionDefault = "Emisión de constancia electrónica fiscal ante el SAR", PrecioDefault = 250, Categoria = "SAR", Activo = true },
                    new ServicioCatalogo { Nombre = "Pagos a Cuenta SAR", DescripcionDefault = "Cálculo y presentación de cuota trimestral de Pagos a Cuenta", PrecioDefault = 400, Categoria = "SAR", Activo = true },
                    new ServicioCatalogo { Nombre = "Impuesto sobre la Renta", DescripcionDefault = "Declaración jurada y liquidación anual de ISR", PrecioDefault = 800, Categoria = "Declaraciones", Activo = true },
                    new ServicioCatalogo { Nombre = "Controles Tributarios", DescripcionDefault = "Revisión y auditoría de control tributario mensual", PrecioDefault = 500, Categoria = "Auditoría", Activo = true },
                    new ServicioCatalogo { Nombre = "Honorarios Mensuales", DescripcionDefault = "Asesoría contable y cumplimiento tributario mensual", PrecioDefault = 600, Categoria = "Honorarios", Activo = true },
                    new ServicioCatalogo { Nombre = "Trámites en Línea SAR", DescripcionDefault = "Gestión de solicitudes y trámites en plataforma SAR", PrecioDefault = 300, Categoria = "SAR", Activo = true }
                };

                await context.ServiciosCatalogo.AddRangeAsync(serviciosDefault);
                await context.SaveChangesAsync();
            }

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

            // Sembrar catálogo de Departamentos y Municipios de Honduras si no existen
            if (!await context.Departamentos.AnyAsync())
            {
                var catalogo = HondurasGeoData.ObtenerCatalogoCompleto();
                foreach (var depDto in catalogo)
                {
                    var dep = new Departamento
                    {
                        Codigo = depDto.Codigo,
                        Nombre = depDto.Nombre,
                        Cabecera = depDto.Cabecera
                    };
                    context.Departamentos.Add(dep);
                    await context.SaveChangesAsync();

                    foreach (var munDto in depDto.Municipios)
                    {
                        var mun = new Municipio
                        {
                            DepartamentoId = dep.Id,
                            Codigo = munDto.Codigo,
                            Nombre = munDto.Nombre
                        };
                        context.Municipios.Add(mun);
                    }
                    await context.SaveChangesAsync();
                }
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

            // Enriquecer histórico para cliente Constructora Lempira si solo tiene 1 período
            var clienteLempira = await context.Clientes.FirstOrDefaultAsync(c => c.Rtn == "05011978998877");
            if (clienteLempira != null)
            {
                var anioActual = System.DateTime.UtcNow.Year;
                var periodosLempira = await context.PeriodosFiscalesSAR.Where(p => p.ClienteId == clienteLempira.Id && p.Anio == anioActual).ToListAsync();
                if (periodosLempira.Count < 6)
                {
                    var mesesHistoricos = new[]
                    {
                        new { Mes = 1, VentasGrav = 85000m, ComprasGrav = 62000m, Dec = "SAR-2026-01-00812" },
                        new { Mes = 2, VentasGrav = 92000m, ComprasGrav = 71000m, Dec = "SAR-2026-02-00933" },
                        new { Mes = 3, VentasGrav = 110000m, ComprasGrav = 80000m, Dec = "SAR-2026-03-01124" },
                        new { Mes = 4, VentasGrav = 105000m, ComprasGrav = 78000m, Dec = "SAR-2026-04-01305" },
                        new { Mes = 5, VentasGrav = 120000m, ComprasGrav = 95000m, Dec = "SAR-2026-05-01512" },
                        new { Mes = 6, VentasGrav = 115000m, ComprasGrav = 88000m, Dec = "SAR-2026-06-01789" },
                        new { Mes = 7, VentasGrav = 130000m, ComprasGrav = 100000m, Dec = "SAR-2026-07-02011" },
                        new { Mes = 8, VentasGrav = 125000m, ComprasGrav = 90000m, Dec = "SAR-2026-08-02240" },
                        new { Mes = 9, VentasGrav = 142500m, ComprasGrav = 110000m, Dec = "SAR-DEC-202609-00941" }
                    };

                    foreach (var m in mesesHistoricos)
                    {
                        var existente = periodosLempira.FirstOrDefault(p => p.Mes == m.Mes);
                        var debito = m.VentasGrav * 0.15m;
                        var credito = m.ComprasGrav * 0.15m;
                        var impuesto = debito - credito;

                        if (existente == null)
                        {
                            var nuevo = new PeriodoFiscalSAR
                            {
                                ClienteId = clienteLempira.Id,
                                Mes = m.Mes,
                                Anio = anioActual,
                                FacturasRecibidas = true,
                                FechaRecepcionFacturas = new DateTime(anioActual, m.Mes, 5, 10, 0, 0, DateTimeKind.Utc),
                                CantidadFacturasVenta = 15 + m.Mes * 2,
                                CantidadFacturasCompra = 20 + m.Mes * 3,
                                VentasGravadas15 = m.VentasGrav,
                                IsvDebito15 = debito,
                                TotalDebitoFiscal = debito,
                                ComprasGravadas15 = m.ComprasGrav,
                                IsvCredito15 = credito,
                                TotalCreditoFiscal = credito,
                                ImpuestoDeterminadoPagar = impuesto > 0 ? impuesto : 0,
                                SaldoAFavorContribuyente = impuesto < 0 ? Math.Abs(impuesto) : 0,
                                LiquidadoSAR = true,
                                FechaLiquidacion = new DateTime(anioActual, m.Mes, 9, 15, 30, 0, DateTimeKind.Utc),
                                NumeroDeclaracionSAR = m.Dec,
                                MontoImpuestoISV = impuesto > 0 ? impuesto : 0,
                                Estado = "Declarado",
                                FechaCreacion = System.DateTime.UtcNow,
                                CreadoPor = "sistema"
                            };
                            context.PeriodosFiscalesSAR.Add(nuevo);
                        }
                        else
                        {
                            existente.VentasGravadas15 = m.VentasGrav;
                            existente.IsvDebito15 = debito;
                            existente.TotalDebitoFiscal = debito;
                            existente.ComprasGravadas15 = m.ComprasGrav;
                            existente.IsvCredito15 = credito;
                            existente.TotalCreditoFiscal = credito;
                            existente.ImpuestoDeterminadoPagar = impuesto > 0 ? impuesto : 0;
                            existente.SaldoAFavorContribuyente = impuesto < 0 ? Math.Abs(impuesto) : 0;
                            existente.LiquidadoSAR = true;
                            existente.FechaLiquidacion = new DateTime(anioActual, m.Mes, 9, 15, 30, 0, DateTimeKind.Utc);
                            existente.NumeroDeclaracionSAR = m.Dec;
                            existente.MontoImpuestoISV = impuesto > 0 ? impuesto : 0;
                            existente.Estado = "Declarado";
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

