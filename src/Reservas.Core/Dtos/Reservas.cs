using Reservas.Core.Domain;

namespace Reservas.Core.Dtos;

public sealed record CrearReservaRequest(
    int EspacioId,
    DateTime Inicio,
    DateTime Fin);

public sealed record RechazarReservaRequest(string? Motivo);

public sealed record CancelarReservaRequest(string? Motivo);

public sealed record ReservaResponse(
    int Id,
    int EspacioId,
    string Espacio,
    int UsuarioId,
    string Usuario,
    string EmailUsuario,
    DateTime Inicio,
    DateTime Fin,
    EstadoReserva Estado,
    string? MotivoRechazo,
    string? MotivoCancelacion,
    DateTime CreadaEn,
    DateTime? DecididaEn,
    int? DecididaPorId,
    DateTime? CanceladaEn);

public sealed record FranjaDisponibilidadResponse(
    int Indice,
    DateTime Inicio,
    DateTime Fin,
    bool Disponible);

public sealed record DisponibilidadResponse(
    int EspacioId,
    string Espacio,
    DateOnly Fecha,
    IReadOnlyList<FranjaDisponibilidadResponse> Franjas);
