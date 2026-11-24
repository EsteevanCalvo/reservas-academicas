namespace Reservas.Core.Domain;

/// <summary>
/// Persona que usa el sistema. El correo es el identificador de acceso y se
/// normaliza a minúsculas para que la comparación no dependa de cómo lo escriba
/// el usuario. La contraseña nunca se guarda en claro: solo su hash.
/// </summary>
public sealed class Usuario
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string HashContrasena { get; private set; } = string.Empty;
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime CreadoEn { get; private set; }

    private Usuario()
    {
        // Constructor requerido por EF Core.
    }

    public Usuario(string nombre, string email, string hashContrasena, Rol rol)
    {
        CambiarNombre(nombre);
        Email = NormalizarEmail(email);
        CambiarContrasena(hashContrasena);
        Rol = rol;
        Activo = true;
        CreadoEn = DateTime.UtcNow;
    }

    /// <summary>
    /// Normaliza y valida un correo. Se usa tanto al crear usuarios como al
    /// iniciar sesión, para que ambos caminos apliquen la misma regla.
    /// </summary>
    public static string NormalizarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ReglaNegocioException("El correo electrónico es obligatorio.");

        string limpio = email.Trim().ToLowerInvariant();

        if (!limpio.Contains('@') || limpio.StartsWith('@') || limpio.EndsWith('@'))
            throw new ReglaNegocioException("El correo electrónico no tiene un formato válido.");

        return limpio;
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("El nombre del usuario es obligatorio.");

        Nombre = nombre.Trim();
    }

    public void CambiarRol(Rol rol) => Rol = rol;

    public void CambiarContrasena(string hashContrasena)
    {
        if (string.IsNullOrWhiteSpace(hashContrasena))
            throw new ReglaNegocioException("El hash de la contraseña es obligatorio.");

        HashContrasena = hashContrasena;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
