using Reservas.Api.Seguridad;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Core.Servicios;

namespace Reservas.Api.Endpoints;

public static class EndpointsReservas
{
    public static void Mapear(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder grupo = app.MapGroup("/api/reservas")
            .WithTags("Reservas")
            .RequireAuthorization();

        grupo.MapPost("/", async (
            CrearReservaRequest peticion,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = contexto.Actual();
            Reserva reserva = await servicio.CrearAsync(usuario.Id, peticion, ct);

            return Results.Created($"/api/reservas/{reserva.Id}", ARespuesta(reserva));
        })
        .RequireAuthorization(Politicas.Solicitante)
        .WithSummary("Crea una solicitud de reserva (Docente o Estudiante)")
        .Produces<ReservaResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapGet("/mias", async (
            EstadoReserva? estado,
            int? espacioId,
            DateTime? desde,
            DateTime? hasta,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = contexto.Actual();

            IReadOnlyList<Reserva> reservas = await servicio.ListarPropiasAsync(
                usuario.Id, estado, espacioId, desde, hasta, ct);

            return Results.Ok(reservas.Select(ARespuesta));
        })
        .WithSummary("Lista el historial del usuario autenticado")
        .Produces<IEnumerable<ReservaResponse>>();

        grupo.MapGet("/", async (
            EstadoReserva? estado,
            int? espacioId,
            DateTime? desde,
            DateTime? hasta,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            IReadOnlyList<Reserva> reservas = await servicio.ListarTodasAsync(
                estado, espacioId, desde, hasta, ct);

            return Results.Ok(reservas.Select(ARespuesta));
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Lista todas las reservas con filtros (solo Administrador)")
        .Produces<IEnumerable<ReservaResponse>>();

        grupo.MapGet("/{id:int}", async (
            int id,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = contexto.Actual();

            Reserva reserva = await servicio.ObtenerAsync(
                id, usuario.Id, usuario.Rol == Rol.Administrador, ct);

            return Results.Ok(ARespuesta(reserva));
        })
        .WithSummary("Obtiene una reserva por identificador (dueño o Administrador)")
        .Produces<ReservaResponse>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/{id:int}/aprobar", async (
            int id,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Reserva reserva = await servicio.AprobarAsync(id, contexto.Actual().Id, ct);
            return Results.Ok(ARespuesta(reserva));
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Aprueba una solicitud pendiente (solo Administrador)")
        .Produces<ReservaResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("/{id:int}/rechazar", async (
            int id,
            RechazarReservaRequest peticion,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Reserva reserva = await servicio.RechazarAsync(
                id, contexto.Actual().Id, peticion.Motivo, ct);

            return Results.Ok(ARespuesta(reserva));
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Rechaza una solicitud pendiente con motivo (solo Administrador)")
        .Produces<ReservaResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/{id:int}/cancelar", async (
            int id,
            CancelarReservaRequest peticion,
            HttpContext contexto,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = contexto.Actual();

            Reserva reserva = await servicio.CancelarAsync(
                id,
                usuario.Id,
                usuario.Rol == Rol.Administrador,
                peticion.Motivo,
                ct);

            return Results.Ok(ARespuesta(reserva));
        })
        .WithSummary("Cancela una reserva con motivo (dueño o Administrador)")
        .Produces<ReservaResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ReservaResponse ARespuesta(Reserva reserva) =>
        new(
            reserva.Id,
            reserva.EspacioId,
            reserva.Espacio?.Nombre ?? string.Empty,
            reserva.UsuarioId,
            reserva.Usuario?.Nombre ?? string.Empty,
            reserva.Usuario?.Email ?? string.Empty,
            reserva.Inicio,
            reserva.Fin,
            reserva.Estado,
            reserva.MotivoRechazo,
            reserva.MotivoCancelacion,
            reserva.CreadaEn,
            reserva.DecididaEn,
            reserva.DecididaPorId,
            reserva.CanceladaEn);
}
