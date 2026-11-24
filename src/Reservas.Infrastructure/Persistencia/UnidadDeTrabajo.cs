using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

public sealed class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private const string NombreIndiceFranjas = "IX_franjas_reservadas_unicas";

    private readonly ReservasDbContext _db;

    public UnidadDeTrabajo(ReservasDbContext db)
    {
        _db = db;
    }

    public async Task<int> GuardarAsync(CancellationToken ct)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EsConflictoDeFranja(ex))
        {
            // Dos peticiones simultáneas intentaron ocupar la misma franja.
            // La base rechazó la segunda: es un conflicto de negocio, no un 500.
            throw new ConflictoException("El espacio ya está reservado en ese horario.");
        }
        catch (DbUpdateException)
        {
            throw new ConflictoException("La operación entra en conflicto con datos existentes.");
        }
    }

    private static bool EsConflictoDeFranja(DbUpdateException ex) =>
        ex.InnerException is SqliteException sqlite
        && sqlite.SqliteErrorCode == 19
        && sqlite.Message.Contains(NombreIndiceFranjas, StringComparison.OrdinalIgnoreCase);
}
