using Reservas.Core.Abstractions;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;

namespace Reservas.Core.Servicios;

public sealed class ServicioReservas
{
    private readonly IRepositorioReservas _reservas;
    private readonly IRepositorioEspacios _espacios;
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajo _unidad;
    private readonly IProveedorFechaHora _reloj;

    public ServicioReservas(
        IRepositorioReservas reservas,
        IRepositorioEspacios espacios,
        IRepositorioUsuarios usuarios,
        IUnidadDeTrabajo unidad,
        IProveedorFechaHora reloj)
    {
        _reservas = reservas;
        _espacios = espacios;
        _usuarios = usuarios;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Reserva> CrearAsync(
        int usuarioId,
        CrearReservaRequest peticion,
        CancellationToken ct)
    {
        Usuario usuario = await _usuarios.ObtenerAsync(usuarioId, ct)
            ?? throw new NoEncontradoException("El usuario autenticado ya no existe.");

        if (!usuario.Activo)
            throw new AccesoDenegadoException("El usuario está desactivado.");

        Espacio espacio = await _espacios.ObtenerAsync(peticion.EspacioId, ct)
            ?? throw new NoEncontradoException(
                $"No se encontró el espacio con identificador {peticion.EspacioId}.");

        if (!espacio.Activo)
            throw new ReglaNegocioException("El espacio está desactivado y no admite reservas.");

        DateTime ahora = _reloj.Ahora;
        ReglasDeReserva.Validar(peticion.Inicio, peticion.Fin, ahora);

        if (await _reservas.ExisteSolapamientoAsync(
                espacio.Id, peticion.Inicio, peticion.Fin, null, ct))
        {
            throw new ConflictoException(
                "El espacio ya tiene una reserva activa que se cruza con ese horario.");
        }

        DateOnly fecha = DateOnly.FromDateTime(peticion.Inicio);

        // Límite de cuatro horas consecutivas: se suman las reservas activas
        // del mismo usuario en el mismo espacio que tocan o solapan la nueva.
        IReadOnlyList<Reserva> activasDelUsuario = await _reservas.ListarActivasDeUsuarioAsync(
            usuarioId, espacio.Id, fecha, ct);

        ReglasDeReserva.ValidarLimiteConsecutivo(
            activasDelUsuario.Select(r => (r.Inicio, r.Fin)),
            peticion.Inicio,
            peticion.Fin);

        var reserva = new Reserva(espacio.Id, usuarioId, peticion.Inicio, peticion.Fin, ahora);

        foreach (int indice in HorarioInstitucional.FranjasEntre(peticion.Inicio, peticion.Fin))
            reserva.AgregarFranja(espacio.Id, fecha, indice);

        _reservas.Agregar(reserva);

        // Si dos peticiones simultáneas llegan hasta aquí, el índice único de
        // franjas rechaza la segunda y la unidad de trabajo la traduce a un
        // conflicto de negocio.
        await _unidad.GuardarAsync(ct);

        return await _reservas.ObtenerAsync(reserva.Id, ct) ?? reserva;
    }

    public async Task<Reserva> ObtenerAsync(
        int reservaId,
        int usuarioId,
        bool esAdministrador,
        CancellationToken ct)
    {
        Reserva reserva = await ObtenerConFranjasAsync(reservaId, ct);

        if (!esAdministrador && reserva.UsuarioId != usuarioId)
            throw new AccesoDenegadoException("No tiene permiso para consultar esta reserva.");

        return reserva;
    }

    public Task<IReadOnlyList<Reserva>> ListarPropiasAsync(
        int usuarioId,
        EstadoReserva? estado,
        int? espacioId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken ct) =>
        _reservas.ListarAsync(
            new FiltroReservas(usuarioId, estado, espacioId, desde, hasta),
            ct);

    public Task<IReadOnlyList<Reserva>> ListarTodasAsync(
        EstadoReserva? estado,
        int? espacioId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken ct) =>
        _reservas.ListarAsync(
            new FiltroReservas(null, estado, espacioId, desde, hasta),
            ct);

    public async Task<DisponibilidadResponse> ConsultarDisponibilidadAsync(
        int espacioId,
        DateOnly fecha,
        CancellationToken ct)
    {
        Espacio espacio = await _espacios.ObtenerAsync(espacioId, ct)
            ?? throw new NoEncontradoException(
                $"No se encontró el espacio con identificador {espacioId}.");

        IReadOnlyList<int> ocupadas = await _reservas.FranjasOcupadasAsync(espacioId, fecha, ct);
        HashSet<int> conjuntoOcupado = ocupadas.ToHashSet();
        DateTime ahora = _reloj.Ahora;

        var franjas = new List<FranjaDisponibilidadResponse>(HorarioInstitucional.TotalFranjas);

        for (int indice = 0; indice < HorarioInstitucional.TotalFranjas; indice++)
        {
            DateTime inicio = HorarioInstitucional.InicioDeFranja(fecha, indice);
            DateTime fin = HorarioInstitucional.FinDeFranja(fecha, indice);

            bool disponible = !conjuntoOcupado.Contains(indice) && inicio > ahora;

            franjas.Add(new FranjaDisponibilidadResponse(indice, inicio, fin, disponible));
        }

        return new DisponibilidadResponse(espacio.Id, espacio.Nombre, fecha, franjas);
    }

    public async Task<Reserva> AprobarAsync(
        int reservaId,
        int administradorId,
        CancellationToken ct)
    {
        Reserva reserva = await ObtenerConFranjasAsync(reservaId, ct);

        // Defensa en profundidad: en condiciones normales la franja está retenida
        // desde la solicitud, pero se revalida antes de comprometer el espacio.
        if (await _reservas.ExisteSolapamientoAsync(
                reserva.EspacioId, reserva.Inicio, reserva.Fin, reserva.Id, ct))
        {
            throw new ConflictoException(
                "Otra reserva activa ocupa esa franja; no se puede aprobar esta solicitud.");
        }

        reserva.Aprobar(administradorId, _reloj.Ahora);
        await _unidad.GuardarAsync(ct);

        return reserva;
    }

    public async Task<Reserva> RechazarAsync(
        int reservaId,
        int administradorId,
        string? motivo,
        CancellationToken ct)
    {
        Reserva reserva = await ObtenerConFranjasAsync(reservaId, ct);

        reserva.Rechazar(administradorId, motivo ?? string.Empty, _reloj.Ahora);
        await _unidad.GuardarAsync(ct);

        return reserva;
    }

    public async Task<Reserva> CancelarAsync(
        int reservaId,
        int usuarioId,
        bool esAdministrador,
        string? motivo,
        CancellationToken ct)
    {
        Reserva reserva = await ObtenerConFranjasAsync(reservaId, ct);

        if (!esAdministrador && reserva.UsuarioId != usuarioId)
            throw new AccesoDenegadoException(
                "Solo el dueño de la reserva o un administrador puede cancelarla.");

        reserva.Cancelar(usuarioId, motivo ?? string.Empty, _reloj.Ahora);
        await _unidad.GuardarAsync(ct);

        return reserva;
    }

    private async Task<Reserva> ObtenerConFranjasAsync(int reservaId, CancellationToken ct)
    {
        Reserva? reserva = await _reservas.ObtenerConFranjasAsync(reservaId, ct);

        return reserva
            ?? throw new NoEncontradoException(
                $"No se encontró la reserva con identificador {reservaId}.");
    }
}
