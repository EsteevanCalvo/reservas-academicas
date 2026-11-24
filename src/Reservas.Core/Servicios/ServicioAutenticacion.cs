using Reservas.Core.Abstractions;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;

namespace Reservas.Core.Servicios;

public sealed class ServicioAutenticacion
{
    private const string MensajeCredenciales = "Las credenciales no son válidas.";

    private readonly IRepositorioUsuarios _usuarios;
    private readonly IHashContrasena _hash;
    private readonly IEmisorTokens _tokens;

    public ServicioAutenticacion(
        IRepositorioUsuarios usuarios,
        IHashContrasena hash,
        IEmisorTokens tokens)
    {
        _usuarios = usuarios;
        _hash = hash;
        _tokens = tokens;
    }

    public async Task<LoginResponse> IniciarSesionAsync(LoginRequest peticion, CancellationToken ct)
    {
        string email = (peticion.Email ?? string.Empty).Trim().ToLowerInvariant();
        string contrasena = peticion.Contrasena ?? string.Empty;

        // Un correo mal formado no debe revelar nada distinto de una contraseña
        // incorrecta: el mensaje es exactamente el mismo.
        if (email.Length == 0 || !email.Contains('@'))
            throw new CredencialesInvalidasException(MensajeCredenciales);

        Usuario? usuario = await _usuarios.ObtenerPorEmailAsync(email, ct);

        if (usuario is null || !usuario.Activo || !_hash.Verificar(contrasena, usuario.HashContrasena))
            throw new CredencialesInvalidasException(MensajeCredenciales);

        (string token, DateTime expira) = _tokens.Emitir(usuario);

        var datos = new UsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol,
            usuario.Activo);

        return new LoginResponse(token, expira, datos);
    }
}
