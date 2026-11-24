using Reservas.Core.Domain;

namespace Reservas.Core.Abstractions;

/// <summary>
/// Filtros del historial. Un filtro nulo no restringe.
/// </summary>
public sealed record FiltroReservas(
    int? UsuarioId,
    EstadoReserva? Estado,
    int? EspacioId,
    DateTime? Desde,
    DateTime? Hasta);

public interface IRepositorioReservas
{
    Task<IReadOnlyList<Reserva>> ListarAsync(FiltroReservas filtro, CancellationToken ct);

    Task<Reserva?> ObtenerAsync(int id, CancellationToken ct);

    Task<Reserva?> ObtenerConFranjasAsync(int id, CancellationToken ct);

    Task<bool> ExisteSolapamientoAsync(
        int espacioId,
        DateTime inicio,
        DateTime fin,
        int? excluirReservaId,
        CancellationToken ct);

    Task<IReadOnlyList<int>> FranjasOcupadasAsync(int espacioId, DateOnly fecha, CancellationToken ct);

    /// <summary>
    /// Reservas activas de un usuario en un espacio y una fecha. Se usa para
    /// calcular el bloque consecutivo del límite de cuatro horas.
    /// </summary>
    Task<IReadOnlyList<Reserva>> ListarActivasDeUsuarioAsync(
        int usuarioId,
        int espacioId,
        DateOnly fecha,
        CancellationToken ct);

    void Agregar(Reserva reserva);
}
