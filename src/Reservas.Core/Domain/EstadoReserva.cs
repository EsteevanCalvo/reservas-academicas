namespace Reservas.Core.Domain;

public enum EstadoReserva
{
    Pendiente,
    Aprobada,
    Rechazada,
    Cancelada
}

public static class EstadoReservaExtensiones
{
    /// <summary>
    /// Una reserva activa ocupa sus franjas. Las rechazadas y canceladas no,
    /// porque sus franjas se liberan en el momento de la decisión.
    /// </summary>
    public static bool EsActiva(this EstadoReserva estado) =>
        estado is EstadoReserva.Pendiente or EstadoReserva.Aprobada;
}
