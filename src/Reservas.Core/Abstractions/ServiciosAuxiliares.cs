using Reservas.Core.Domain;

namespace Reservas.Core.Abstractions;

/// <summary>
/// Servicios auxiliares que dependen de la plataforma (criptografía, tokens).
/// </summary>
public interface IHashContrasena
{
    string Hashear(string contrasena);

    bool Verificar(string contrasena, string hashAlmacenado);
}

public interface IEmisorTokens
{
    (string Token, DateTime Expira) Emitir(Usuario usuario);
}
