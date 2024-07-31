using API.Modelo;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Alerta> Alertas { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Orden> Órdenes { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
    public DbSet<Categoría> Categorías { get; set; }
    public DbSet<Dirección> Direcciones { get; set; }
    public DbSet<MétodoPago> MétodosPago { get; set; }
    public DbSet<HorasSeleccionadas> HorasSeleccionadas { get; set; }
    public DbSet<Monitoreo> Monitoreo { get; set; }

    // Añadir DbSet para HorasSeleccionadas
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DetalleOrden>()
            .HasKey(d => d.DetalleOrdenId);

        modelBuilder.Entity<Monitoreo>(entity =>
        {
            entity.HasKey(e => e.ID_M);
            entity.Property(e => e.Temperatura).IsRequired();
            entity.Property(e => e.tds).IsRequired();
            entity.Property(e => e.PH).IsRequired();
            entity.Property(e => e.FechaHora).IsRequired();
            entity.Property(e => e.LoteID).IsRequired();
            entity.Property(e => e.userId).IsRequired();
        });

        modelBuilder.Entity<Categoría>(entity =>
        {
            entity.HasKey(e => e.CategoríaId);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Alerta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).IsRequired();
        });

        // Configuraciones adicionales si es necesario
    }
}
