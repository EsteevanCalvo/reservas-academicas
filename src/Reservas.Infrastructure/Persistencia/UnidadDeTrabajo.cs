using Reservas.Core.Abstractions;

namespace Reservas.Infrastructure.Persistencia;

public sealed class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly ReservasDbContext _db;

    public UnidadDeTrabajo(ReservasDbContext db)
    {
        _db = db;
    }

    public Task<int> GuardarAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
