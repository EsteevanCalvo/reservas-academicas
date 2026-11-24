using Microsoft.EntityFrameworkCore;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

public sealed class RepositorioEspacios : IRepositorioEspacios
{
    private readonly ReservasDbContext _db;

    public RepositorioEspacios(ReservasDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Espacio>> ListarAsync(TipoEspacio? tipo, bool? activo, CancellationToken ct)
    {
        IQueryable<Espacio> consulta = _db.Espacios.AsNoTracking();

        if (tipo is not null)
            consulta = consulta.Where(e => e.Tipo == tipo);

        if (activo is not null)
            consulta = consulta.Where(e => e.Activo == activo);

        return await consulta.OrderBy(e => e.Nombre).ToListAsync(ct);
    }

    public Task<Espacio?> ObtenerAsync(int id, CancellationToken ct) =>
        _db.Espacios.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct) =>
        _db.Espacios.AnyAsync(
            e => e.Nombre == nombre && (excluirId == null || e.Id != excluirId),
            ct);

    public void Agregar(Espacio espacio) => _db.Espacios.Add(espacio);
}
