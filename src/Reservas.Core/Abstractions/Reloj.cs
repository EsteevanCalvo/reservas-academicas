namespace Reservas.Core.Abstractions;

/// <summary>
/// Reloj del sistema. Se abstrae para que las reglas que dependen del momento
/// actual se puedan probar sin esperar.
/// </summary>
public interface IProveedorFechaHora
{
    DateTime Ahora { get; }
}
