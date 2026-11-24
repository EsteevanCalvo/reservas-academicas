using Reservas.Core.Abstractions;

namespace Reservas.Infrastructure.Tiempo;

/// <summary>
/// Reloj del servidor. Se usa la hora local porque toda la institución está en
/// una única zona horaria; el contrato no acepta desplazamientos.
/// </summary>
public sealed class RelojSistema : IProveedorFechaHora
{
    public DateTime Ahora => DateTime.Now;
}
