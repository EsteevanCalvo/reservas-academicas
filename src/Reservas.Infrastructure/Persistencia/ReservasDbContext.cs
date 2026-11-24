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
    }
}
