using API.Modelo;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EspecieLote> EspecieLote { get; set; }
    public DbSet<CrearEspecie> CrearEspecie { get; set; }
    public DbSet<Alerta> Alertas { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Orden> Órdenes { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
    public DbSet<Categoría> Categorías { get; set; }
    public DbSet<Dirección> Direcciones { get; set; }
    public DbSet<MétodoPago> MétodosPago { get; set; }
    public DbSet<Monitoreo> Monitoreo { get; set; }
    public DbSet<HorasSeleccionadas> HorasSeleccionadas { get; set; }
    public DbSet<SensorData> SensorData { get; set; } // Añade esta línea

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.RequirePasswordChange)
            .HasDefaultValue(false);

        modelBuilder.Entity<DetalleOrden>()
            .HasKey(d => d.DetalleOrdenId);

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripción).HasMaxLength(500);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CategoríaId).IsRequired();
            entity.HasOne(e => e.Categoría)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(e => e.CategoríaId);
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

        modelBuilder.Entity<SensorData>()
            .HasKey(sd => sd.Id); // Configura 'Id' como la clave primaria

        // Configuraciones adicionales si es necesario
    }
}
