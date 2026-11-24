using Reservas.Core.Abstractions;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;

namespace Reservas.Core.Servicios;

public sealed class ServicioUsuarios
{
    public const int LongitudMinimaContrasena = 8;

    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajo _unidad;
    private readonly IHashContrasena _hash;

    public ServicioUsuarios(
        IRepositorioUsuarios usuarios,
        IUnidadDeTrabajo unidad,
        IHashContrasena hash)
    {
        _usuarios = usuarios;
        _unidad = unidad;
        _hash = hash;
    }

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct) =>
        _usuarios.ListarAsync(ct);

    public async Task<Usuario> ObtenerAsync(int id, CancellationToken ct)
    {
        Usuario? usuario = await _usuarios.ObtenerAsync(id, ct);

        if (usuario is null)
            throw new NoEncontradoException($"No se encontró el usuario con identificador {id}.");

        return usuario;
    }

    public async Task<Usuario> CrearAsync(CrearUsuarioRequest peticion, CancellationToken ct)
    {
        ValidarRol(peticion.Rol);
        string contrasena = peticion.Contrasena ?? string.Empty;
        ValidarContrasena(contrasena);

        string email = Usuario.NormalizarEmail(peticion.Email);

        if (await _usuarios.ExisteEmailAsync(email, null, ct))
            throw new ConflictoException($"Ya existe un usuario con el correo '{email}'.");

        var usuario = new Usuario(
            peticion.Nombre ?? string.Empty,
            email,
            _hash.Hashear(contrasena),
            peticion.Rol);

        _usuarios.Agregar(usuario);
        await _unidad.GuardarAsync(ct);
        return usuario;
    }

    public async Task<Usuario> ActualizarAsync(int id, ActualizarUsuarioRequest peticion, CancellationToken ct)
    {
        ValidarRol(peticion.Rol);
        Usuario usuario = await ObtenerAsync(id, ct);

        usuario.CambiarNombre(peticion.Nombre ?? string.Empty);
        usuario.CambiarRol(peticion.Rol);

        if (peticion.Activo)
            usuario.Activar();
        else
            usuario.Desactivar();

        await _unidad.GuardarAsync(ct);
        return usuario;
    }

    public async Task RestablecerContrasenaAsync(int id, RestablecerContrasenaRequest peticion, CancellationToken ct)
    {
        Usuario usuario = await ObtenerAsync(id, ct);
        string contrasena = peticion.Contrasena ?? string.Empty;
        ValidarContrasena(contrasena);

        usuario.CambiarContrasena(_hash.Hashear(contrasena));
        await _unidad.GuardarAsync(ct);
    }

    private static void ValidarContrasena(string contrasena)
    {
        if (contrasena.Length < LongitudMinimaContrasena)
            throw new ReglaNegocioException(
                $"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.");
    }

    private static void ValidarRol(Rol rol)
    {
        if (!Enum.IsDefined(rol))
            throw new ReglaNegocioException("El rol indicado no es válido.");
    }
}
