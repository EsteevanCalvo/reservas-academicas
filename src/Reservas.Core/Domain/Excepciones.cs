namespace Reservas.Core.Domain;

/// <summary>
/// Errores de negocio. La capa de presentación los traduce a códigos HTTP;
/// el dominio no sabe nada de HTTP ni de la base de datos.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

public sealed class NoEncontradoException : Exception
{
    public NoEncontradoException(string mensaje) : base(mensaje) { }
}

public sealed class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje) { }
}

public sealed class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string mensaje) : base(mensaje) { }
}

public sealed class CredencialesInvalidasException : Exception
{
    public CredencialesInvalidasException(string mensaje) : base(mensaje) { }
}
