namespace Reservas.Core.Domain;

/// <summary>
/// Espacio académico reservable: laboratorio, auditorio o sala de cómputo.
/// La entidad protege sus propias invariantes: no se puede construir ni
/// modificar con un nombre vacío o una capacidad no positiva.
/// </summary>
public sealed class Espacio
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public TipoEspacio Tipo { get; private set; }
    public int Capacidad { get; private set; }
    public string Ubicacion { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    private Espacio()
    {
        // Constructor requerido por EF Core.
    }

    public Espacio(string nombre, TipoEspacio tipo, int capacidad, string? ubicacion)
    {
        CambiarNombre(nombre);
        Tipo = tipo;
        CambiarCapacidad(capacidad);
        Ubicacion = (ubicacion ?? string.Empty).Trim();
        Activo = true;
    }

    public void Actualizar(string nombre, TipoEspacio tipo, int capacidad, string? ubicacion)
    {
        CambiarNombre(nombre);
        Tipo = tipo;
        CambiarCapacidad(capacidad);
        Ubicacion = (ubicacion ?? string.Empty).Trim();
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;

    private void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("El nombre del espacio es obligatorio.");

        Nombre = nombre.Trim();
    }

    private void CambiarCapacidad(int capacidad)
    {
        if (capacidad <= 0)
            throw new ReglaNegocioException("La capacidad del espacio debe ser mayor que cero.");

        Capacidad = capacidad;
    }
}
