using System;
using System.Threading;
using System.Threading.Tasks;
using ContaFlow.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Data
{
    public class ContaFlowDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContaFlowDbContext(DbContextOptions<ContaFlowDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<PagoHonorario> PagosHonorarios => Set<PagoHonorario>();
        public DbSet<Recibo> Recibos => Set<Recibo>();
        public DbSet<PeriodoFiscalSAR> PeriodosFiscalesSAR => Set<PeriodoFiscalSAR>();
        public DbSet<ConfiguracionDespacho> ConfiguracionDespacho => Set<ConfiguracionDespacho>();
        public DbSet<AutorizacionCAI> AutorizacionesCAI => Set<AutorizacionCAI>();
        public DbSet<SeguimientoFiscalAnual> SeguimientosFiscalesAnuales => Set<SeguimientoFiscalAnual>();
        public DbSet<Rubro> Rubros => Set<Rubro>();
        public DbSet<LibroDetalleItem> LibroDetalleItems => Set<LibroDetalleItem>();
        public DbSet<LibroVentaDetalleItem> LibrosVentasItems => Set<LibroVentaDetalleItem>();
        public DbSet<LibroCompraDetalleItem> LibrosComprasItems => Set<LibroCompraDetalleItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tablas en PostgreSQL
            modelBuilder.Entity<Usuario>().ToTable("usuarios").HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Cliente>().ToTable("clientes").HasIndex(c => c.Rtn).IsUnique();
            modelBuilder.Entity<PagoHonorario>().ToTable("pagos_honorarios");
            modelBuilder.Entity<Recibo>().ToTable("recibos").HasIndex(r => r.NumeroRecibo).IsUnique();
            modelBuilder.Entity<PeriodoFiscalSAR>().ToTable("periodos_fiscales_sar");
            modelBuilder.Entity<ConfiguracionDespacho>().ToTable("configuracion_despacho");
            modelBuilder.Entity<AutorizacionCAI>().ToTable("autorizaciones_cai");
            modelBuilder.Entity<SeguimientoFiscalAnual>().ToTable("seguimientos_fiscales_anuales");
            modelBuilder.Entity<Rubro>().ToTable("catalogo_rubros").HasIndex(r => r.Nombre).IsUnique();
            modelBuilder.Entity<LibroDetalleItem>().ToTable("libros_detalle_items");
            modelBuilder.Entity<LibroVentaDetalleItem>().ToTable("libros_ventas_items");
            modelBuilder.Entity<LibroCompraDetalleItem>().ToTable("libros_compras_items");

            // Relación LibroVentaDetalleItem -> PeriodoFiscalSAR
            modelBuilder.Entity<LibroVentaDetalleItem>()
                .HasOne(v => v.PeriodoFiscal)
                .WithMany(pf => pf.VentasDetalleItems)
                .HasForeignKey(v => v.PeriodoFiscalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación LibroCompraDetalleItem -> PeriodoFiscalSAR
            modelBuilder.Entity<LibroCompraDetalleItem>()
                .HasOne(c => c.PeriodoFiscal)
                .WithMany(pf => pf.ComprasDetalleItems)
                .HasForeignKey(c => c.PeriodoFiscalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación LibroDetalleItem -> PeriodoFiscalSAR
            modelBuilder.Entity<LibroDetalleItem>()
                .HasOne(d => d.PeriodoFiscal)
                .WithMany(pf => pf.DetalleItems)
                .HasForeignKey(d => d.PeriodoFiscalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación SeguimientoFiscalAnual -> Cliente
            modelBuilder.Entity<SeguimientoFiscalAnual>()
                .HasOne(s => s.Cliente)
                .WithMany()
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Cliente -> PagosHonorarios
            modelBuilder.Entity<PagoHonorario>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación PagoHonorario -> Recibo (1 a 1)
            modelBuilder.Entity<Recibo>()
                .HasOne(r => r.PagoHonorario)
                .WithOne(p => p.Recibo)
                .HasForeignKey<Recibo>(r => r.PagoHonorarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Cliente -> PeriodosFiscalesSAR
            modelBuilder.Entity<PeriodoFiscalSAR>()
                .HasOne(pf => pf.Cliente)
                .WithMany(c => c.PeriodosFiscales)
                .HasForeignKey(pf => pf.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var usuarioActual = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "sistema";
            var ahora = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.FechaCreacion = ahora;
                    entry.Entity.CreadoPor = usuarioActual;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.FechaModificacion = ahora;
                    entry.Entity.ModificadoPor = usuarioActual;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
