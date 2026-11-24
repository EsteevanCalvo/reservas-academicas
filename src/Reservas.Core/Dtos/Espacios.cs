using Reservas.Core.Domain;

namespace Reservas.Core.Dtos;

public sealed record CrearEspacioRequest(
    string? Nombre,
    TipoEspacio Tipo,
    int Capacidad,
    string? Ubicacion);

public sealed record ActualizarEspacioRequest(
    string? Nombre,
    TipoEspacio Tipo,
    int Capacidad,
    string? Ubicacion);

public sealed record EspacioResponse(
    int Id,
    string Nombre,
    TipoEspacio Tipo,
    int Capacidad,
    string Ubicacion,
    bool Activo);
