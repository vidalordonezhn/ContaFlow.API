using System.Text;
using ContaFlow.API.Data;
using ContaFlow.API.Features.Auth;
using ContaFlow.API.Features.Clientes;
using ContaFlow.API.Features.LibrosISV;
using ContaFlow.API.Features.Pagos;
using ContaFlow.API.Features.SARControl;
using ContaFlow.API.Features.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos: Entity Framework Core + PostgreSQL ─────────────
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["DATABASE_URL"] 
    ?? string.Empty;

var effectiveConnectionString = rawConnectionString;

if (!string.IsNullOrWhiteSpace(rawConnectionString) && 
    (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || 
     rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        var uri = new Uri(rawConnectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        effectiveConnectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch
    {
        effectiveConnectionString = rawConnectionString;
    }
}

builder.Services.AddDbContext<ContaFlowDbContext>(options =>
    options.UseNpgsql(effectiveConnectionString));

// ── Acceso al contexto HTTP (para auditoría automática) ───────────
builder.Services.AddHttpContextAccessor();

// ── Servicios de la aplicación ────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<ClientesService>();
builder.Services.AddScoped<SARControlService>();
builder.Services.AddScoped<PagosService>();
builder.Services.AddScoped<LibrosIsvService>();

// ── Autenticación JWT ─────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ContaFlow_SuperSecretKey_2026_DespachoContable_Secret!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ContaFlow.API",
        ValidAudience = jwtSettings["Audience"] ?? "ContaFlow.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// ── CORS para comunicación con el Frontend de Angular (Local y Vercel) ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger con soporte para Autorización JWT ─────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ContaFlow API - Sistema de Gestión Contable y SAR",
        Version = "v1",
        Description = "API de control contable, clientes, recibos de honorarios y liquidación SAR."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer {tu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseDeveloperExceptionPage();

// ── Sembrado automático de base de datos en inicio ────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ContaFlowDbContext>();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar y sembrar la base de datos.");
    }
}

// ── Middleware Pipeline (Swagger siempre activo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContaFlow API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "online", service = "ContaFlow API", timestamp = DateTime.UtcNow }));

app.MapGet("/api/health", async (ContaFlowDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var userCount = canConnect ? await context.Usuarios.CountAsync() : 0;
        return Results.Ok(new
        {
            status = "healthy",
            databaseConnected = canConnect,
            usuariosRegistrados = userCount,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database connection failed",
            detail: ex.ToString(),
            statusCode: 500
        );
    }
});

app.MapControllers();

app.Run();
