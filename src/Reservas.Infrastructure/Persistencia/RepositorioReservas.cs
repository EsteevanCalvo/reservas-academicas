using Microsoft.EntityFrameworkCore;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

public sealed class RepositorioReservas : IRepositorioReservas
{
    private readonly ReservasDbContext _db;

    public RepositorioReservas(ReservasDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Reserva>> ListarAsync(FiltroReservas filtro, CancellationToken ct)
    {
        IQueryable<Reserva> consulta = _db.Reservas
            .AsNoTracking()
            .Include(r => r.Espacio)
            .Include(r => r.Usuario);

        if (filtro.UsuarioId is not null)
            consulta = consulta.Where(r => r.UsuarioId == filtro.UsuarioId);

        if (filtro.Estado is not null)
            consulta = consulta.Where(r => r.Estado == filtro.Estado);

        if (filtro.EspacioId is not null)
            consulta = consulta.Where(r => r.EspacioId == filtro.EspacioId);

        if (filtro.Desde is not null)
            consulta = consulta.Where(r => r.Inicio >= filtro.Desde);

        if (filtro.Hasta is not null)
            consulta = consulta.Where(r => r.Inicio <= filtro.Hasta);

        return await consulta
            .OrderByDescending(r => r.Inicio)
            .ToListAsync(ct);
    }

    public Task<Reserva?> ObtenerAsync(int id, CancellationToken ct) =>
        _db.Reservas
            .AsNoTracking()
            .Include(r => r.Espacio)
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Reserva?> ObtenerConFranjasAsync(int id, CancellationToken ct) =>
        _db.Reservas
            .Include(r => r.Espacio)
            .Include(r => r.Usuario)
            .Include(r => r.Franjas)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExisteSolapamientoAsync(
        int espacioId,
        DateTime inicio,
        DateTime fin,
        int? excluirReservaId,
        CancellationToken ct) =>
        _db.Reservas.AnyAsync(
            r => r.EspacioId == espacioId
                && (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Aprobada)
                && (excluirReservaId == null || r.Id != excluirReservaId)
                && r.Inicio < fin
                && r.Fin > inicio,
            ct);

    public async Task<IReadOnlyList<int>> FranjasOcupadasAsync(
        int espacioId,
        DateOnly fecha,
        CancellationToken ct) =>
        await _db.FranjasReservadas
            .Where(f => f.EspacioId == espacioId && f.Fecha == fecha)
            .Select(f => f.Indice)
            .ToListAsync(ct);

    public void Agregar(Reserva reserva) => _db.Reservas.Add(reserva);
}
