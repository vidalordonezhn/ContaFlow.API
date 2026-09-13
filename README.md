# ⚡ ContaFlow API (Backend)

API RESTful empresarial desarrollada en **.NET 10** y **PostgreSQL** para la gestión contable, emisión fiscal y cumplimiento tributario según las leyes del **SAR (Servicio de Administración de Rentas de Honduras)**.

---

## 🌟 Módulos y Funcionalidades

- **🔐 Autenticación & Seguridad:**
  - JSON Web Tokens (JWT) con roles (`Admin`, `Contador`).
  - Hashing seguro de contraseñas y validación de sesiones.
- **👥 Gestión de Clientes:**
  - Registro y categorización de contribuyentes (RTN, régimen fiscal, personas naturales/jurídicas).
  - Configuración de honorarios contables mensuales.
- **🧾 Control Fiscal CAI y Recibos:**
  - Registro de autorizaciones CAI otorgadas por el SAR.
  - Asignación correlativa estricta (`000-001-01-XXXXXXXX`) con control de concurrencia.
  - Validación de rango autorizado y fecha límite de emisión.
- **📅 Calendario Fiscal Anual:**
  - Matriz de seguimiento para ISR (30 de Abril), Pagos a Cuenta trimestrales y Retenciones.
- **📑 Motor de Libros ISV (SAR-210):**
  - Registro de ventas gravadas al 15%, 18% y exentas (Débito Fiscal).
  - Registro de compras locales e importaciones gravadas al 15%, 18% y exentas (Crédito Fiscal).
  - Liquidación automática SAR-210: deducción de saldo anterior, retenciones recibidas y determinación de impuesto a pagar o saldo a favor.
  - Generación y procesamiento masivo mediante plantillas Excel/CSV.
- **🏢 Perfil del Despacho:**
  - Datos de colegiación CAH, cuentas bancarias para cobros y personalización de recibos.

---

## 🛠️ Tecnologías y Arquitectura

- **Framework:** [.NET 10 SDK](https://dotnet.microsoft.com/) (ASP.NET Core Web API)
- **Lenguaje:** C# 13
- **Base de Datos:** [PostgreSQL](https://www.postgresql.org/)
- **ORM:** [Entity Framework Core 9](https://learn.microsoft.com/ef/core/) (Npgsql.EntityFrameworkCore.PostgreSQL)
- **Seguridad:** JWT Bearer Authentication & BCrypt
- **Documentación API:** Swagger / OpenAPI

---

## 📋 Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (o versión compatible)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [ContaFlow Frontend](../ContaFlow) para la interfaz web

---

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/TU_USUARIO/ContaFlow.API.git
cd ContaFlow.API
```

### 2. Configurar la Base de Datos
Crea una base de datos en PostgreSQL (por ejemplo `contaflow_db`).

Abre el archivo `ContaFlow.API/appsettings.json` (o `appsettings.Development.json`) y configura tu cadena de conexión:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=contaflow_db;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "Key": "ContaFlow_SecretKey_Honduras_2026_SecureKeyExample!",
    "Issuer": "ContaFlowAPI",
    "Audience": "ContaFlowApp"
  }
}
```

### 3. Ejecutar las Migraciones y Seed Inicial
El sistema cuenta con un sembrador automático (`DbSeeder.cs`) que al arrancar crea las tablas, actualiza columnas y crea los datos de ejemplo iniciales automáticamente.

Para compilar y correr:
```bash
cd ContaFlow.API
dotnet restore
dotnet build
dotnet run
```

La API se iniciará por defecto en:
- `http://localhost:5057`
- Documentación interactiva Swagger: `http://localhost:5057/swagger`

---

## 🧪 Estructura del Proyecto

```
ContaFlow.API/
├── ContaFlow.API/
│   ├── Controllers/          # Controladores REST API
│   ├── Data/                 # ContaFlowDbContext y DbSeeder
│   ├── Entities/             # Entidades de base de datos (Cliente, Recibo, PeriodoFiscalSAR...)
│   ├── Features/             # Módulos verticales de negocio (LibrosISV, CalendarioFiscal, CAI...)
│   ├── Migrations/           # Migraciones de Entity Framework Core
│   ├── Properties/           # launchSettings.json
│   ├── Program.cs            # Punto de entrada y configuración de servicios
│   └── appsettings.json      # Configuración de base de datos y JWT
├── ContaFlow.API.slnx        # Solución .NET
└── README.md
```

---

## 📄 Licencia
Este proyecto es privado para uso de despachos contables. Todos los derechos reservados.
