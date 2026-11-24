using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservas.Core.Abstractions;
using Reservas.Infrastructure.Persistencia;
using Reservas.Infrastructure.Seguridad;

namespace Reservas.Infrastructure;

public static class InyeccionDeDependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        string cadenaConexion)
    {
        servicios.AddDbContext<ReservasDbContext>(opciones => opciones.UseSqlite(cadenaConexion));

        servicios.AddScoped<IRepositorioEspacios, RepositorioEspacios>();
        servicios.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        servicios.AddSingleton<IHashContrasena, HashPbkdf2>();
        servicios.AddScoped<IEmisorTokens, EmisorTokens>();

        return servicios;
    }

    /// <summary>
    /// Aplica las migraciones pendientes y, si corresponde, siembra los datos
    /// de demostración. Se ejecuta una vez al arrancar la aplicación.
    /// </summary>
    public static async Task PrepararBaseDeDatosAsync(
        this IServiceProvider proveedor,
        bool sembrar,
        CancellationToken ct = default)
    {
        await using AsyncServiceScope alcance = proveedor.CreateAsyncScope();
        var db = alcance.ServiceProvider.GetRequiredService<ReservasDbContext>();

        await db.Database.MigrateAsync(ct);

        // WAL permite leer mientras se escribe; sin esto, una escritura bloquea
        // todas las lecturas y la prueba de concurrencia se vuelve frágil.
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", ct);

        if (sembrar)
        {
            var hash = alcance.ServiceProvider.GetRequiredService<IHashContrasena>();
            await SemillaDesarrollo.SembrarAsync(db, hash, ct);
        }
    }
}
