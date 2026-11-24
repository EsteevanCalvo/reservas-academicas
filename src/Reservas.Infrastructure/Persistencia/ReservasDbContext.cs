using Microsoft.EntityFrameworkCore;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

public sealed class ReservasDbContext : DbContext
{
    public ReservasDbContext(DbContextOptions<ReservasDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Espacio> Espacios => Set<Espacio>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Reserva> Reservas => Set<Reserva>();

    public DbSet<FranjaReservada> FranjasReservadas => Set<FranjaReservada>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Espacio>(entidad =>
        {
            entidad.ToTable("espacios");
            entidad.HasKey(e => e.Id);

            // NOCASE hace que la unicidad del nombre no dependa de mayúsculas.
            entidad.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsRequired()
                .UseCollation("NOCASE");

            entidad.HasIndex(e => e.Nombre).IsUnique();

            entidad.Property(e => e.Tipo)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entidad.Property(e => e.Capacidad).IsRequired();

            entidad.Property(e => e.Ubicacion)
                .HasMaxLength(200)
                .IsRequired();

            entidad.Property(e => e.Activo).IsRequired();
        });

        modelo.Entity<Usuario>(entidad =>
        {
            entidad.ToTable("usuarios");
            entidad.HasKey(u => u.Id);

            entidad.Property(u => u.Nombre)
                .HasMaxLength(120)
                .IsRequired();

            entidad.Property(u => u.Email)
                .HasMaxLength(200)
                .IsRequired()
                .UseCollation("NOCASE");

            entidad.HasIndex(u => u.Email).IsUnique();

            entidad.Property(u => u.HashContrasena)
                .HasMaxLength(400)
                .IsRequired();

            entidad.Property(u => u.Rol)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entidad.Property(u => u.Activo).IsRequired();
            entidad.Property(u => u.CreadoEn).IsRequired();
        });

        modelo.Entity<Reserva>(entidad =>
        {
            entidad.ToTable("reservas");
            entidad.HasKey(r => r.Id);

            entidad.Property(r => r.Inicio).IsRequired();
            entidad.Property(r => r.Fin).IsRequired();

            entidad.Property(r => r.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidad.Property(r => r.MotivoRechazo).HasMaxLength(500);
            entidad.Property(r => r.MotivoCancelacion).HasMaxLength(500);

            entidad.Property(r => r.CreadaEn).IsRequired();

            // Borrado restringido: un espacio o un usuario con historial no se
            // elimina de la base. Los espacios se desactivan lógicamente.
            entidad.HasOne(r => r.Espacio)
                .WithMany()
                .HasForeignKey(r => r.EspacioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasMany(r => r.Franjas)
                .WithOne()
                .HasForeignKey(f => f.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            entidad.HasIndex(r => new { r.EspacioId, r.Inicio });
            entidad.HasIndex(r => new { r.UsuarioId, r.Inicio });
        });

        modelo.Entity<FranjaReservada>(entidad =>
        {
            entidad.ToTable("franjas_reservadas");
            entidad.HasKey(f => f.Id);

            entidad.Property(f => f.Fecha).IsRequired();
            entidad.Property(f => f.Indice).IsRequired();

            // Garantía de exclusión mutua a nivel de base de datos. Es la pieza
            // que impide dos reservas activas sobre la misma franja, incluso con
            // peticiones concurrentes.
            entidad.HasIndex(f => new { f.EspacioId, f.Fecha, f.Indice })
                .IsUnique()
                .HasDatabaseName("IX_franjas_reservadas_unicas");

            entidad.HasOne<Espacio>()
                .WithMany()
                .HasForeignKey(f => f.EspacioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
