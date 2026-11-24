using Reservas.Core.Domain;

namespace Reservas.Core.Dtos;

public sealed record CrearUsuarioRequest(
    string? Nombre,
    string? Email,
    string? Contrasena,
    Rol Rol);

public sealed record ActualizarUsuarioRequest(
    string? Nombre,
    Rol Rol,
    bool Activo);

public sealed record RestablecerContrasenaRequest(string? Contrasena);

public sealed record UsuarioResponse(
    int Id,
    string Nombre,
    string Email,
    Rol Rol,
    bool Activo);

public sealed record LoginRequest(string? Email, string? Contrasena);

public sealed record LoginResponse(string Token, DateTime Expira, UsuarioResponse Usuario);
