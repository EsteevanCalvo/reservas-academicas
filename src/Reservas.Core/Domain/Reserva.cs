namespace Reservas.Core.Domain;

/// <summary>
/// Solicitud de reserva de un espacio académico. La entidad protege su propia
/// máquina de estados: no se puede aprobar dos veces ni cancelar una reserva
/// rechazada. Rechazar o cancelar libera las franjas ocupadas.
/// </summary>
public sealed class Reserva
{
    public int Id { get; private set; }

    public int EspacioId { get; private set; }

    public Espacio Espacio { get; private set; } = null!;

    public int UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public DateTime Inicio { get; private set; }

    public DateTime Fin { get; private set; }

    public EstadoReserva Estado { get; private set; }

    public string? MotivoRechazo { get; private set; }

    public string? MotivoCancelacion { get; private set; }

    public DateTime CreadaEn { get; private set; }

    public DateTime? DecididaEn { get; private set; }

    public int? DecididaPorId { get; private set; }

    public DateTime? CanceladaEn { get; private set; }

    public int? CanceladaPorId { get; private set; }

    public ICollection<FranjaReservada> Franjas { get; private set; } = new List<FranjaReservada>();

    private Reserva()
    {
        // Constructor requerido por EF Core.
    }

    public Reserva(int espacioId, int usuarioId, DateTime inicio, DateTime fin, DateTime creadaEn)
    {
        EspacioId = espacioId;
        UsuarioId = usuarioId;
        Inicio = inicio;
        Fin = fin;
        CreadaEn = creadaEn;
        Estado = EstadoReserva.Pendiente;
    }

    public void AgregarFranja(int espacioId, DateOnly fecha, int indice) =>
        Franjas.Add(new FranjaReservada(espacioId, fecha, indice));

    public void Aprobar(int administradorId, DateTime momento)
    {
        ExigirEstado(EstadoReserva.Pendiente);

        Estado = EstadoReserva.Aprobada;
        DecididaPorId = administradorId;
        DecididaEn = momento;
    }

    public void Rechazar(int administradorId, string motivo, DateTime momento)
    {
        ExigirEstado(EstadoReserva.Pendiente);

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ReglaNegocioException("El motivo del rechazo es obligatorio.");

        Estado = EstadoReserva.Rechazada;
        MotivoRechazo = motivo.Trim();
        DecididaPorId = administradorId;
        DecididaEn = momento;

        // La franja deja de estar ocupada: otra persona puede solicitarla.
        Franjas.Clear();
    }

    public void Cancelar(int usuarioId, string motivo, DateTime momento)
    {
        if (!Estado.EsActiva())
            throw new ReglaNegocioException(
                $"No se puede cancelar una reserva en estado {Estado}.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ReglaNegocioException("El motivo de la cancelación es obligatorio.");

        Estado = EstadoReserva.Cancelada;
        MotivoCancelacion = motivo.Trim();
        CanceladaPorId = usuarioId;
        CanceladaEn = momento;

        Franjas.Clear();
    }

    private void ExigirEstado(EstadoReserva esperado)
    {
        if (Estado != esperado)
            throw new ReglaNegocioException(
                $"La reserva está en estado {Estado} y la operación requiere el estado {esperado}.");
    }
}
