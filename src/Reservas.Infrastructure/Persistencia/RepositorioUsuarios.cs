using Microsoft.EntityFrameworkCore;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

public sealed class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly ReservasDbContext _db;

    public RepositorioUsuarios(ReservasDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct) =>
        await _db.Usuarios.AsNoTracking().OrderBy(u => u.Nombre).ToListAsync(ct);

    public Task<Usuario?> ObtenerAsync(int id, CancellationToken ct) =>
        _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct) =>
        _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct) =>
        _db.Usuarios.AnyAsync(
            u => u.Email == email && (excluirId == null || u.Id != excluirId),
            ct);

    public void Agregar(Usuario usuario) => _db.Usuarios.Add(usuario);
}
