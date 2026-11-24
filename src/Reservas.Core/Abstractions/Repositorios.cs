using Reservas.Core.Domain;

namespace Reservas.Core.Abstractions;

/// <summary>
/// Contrato de persistencia de espacios. El dominio depende de esta abstracción,
/// nunca de EF Core ni de SQLite.
/// </summary>
public interface IRepositorioEspacios
{
    Task<IReadOnlyList<Espacio>> ListarAsync(TipoEspacio? tipo, bool? activo, CancellationToken ct);

    Task<Espacio?> ObtenerAsync(int id, CancellationToken ct);

    Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct);

    void Agregar(Espacio espacio);
}

public interface IRepositorioUsuarios
{
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct);

    Task<Usuario?> ObtenerAsync(int id, CancellationToken ct);

    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct);

    Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct);

    void Agregar(Usuario usuario);
}

/// <summary>
/// Confirma los cambios de una unidad de trabajo. Se abstrae para que los
/// servicios de aplicación no dependan de DbContext.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task<int> GuardarAsync(CancellationToken ct);
}
