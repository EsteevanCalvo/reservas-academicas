using Reservas.Core.Dtos;
using Reservas.Core.Servicios;

namespace Reservas.Api.Endpoints;

public static class EndpointsDisponibilidad
{
    public static void Mapear(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/disponibilidad", async (
            int espacioId,
            DateOnly fecha,
            ServicioReservas servicio,
            CancellationToken ct) =>
        {
            DisponibilidadResponse respuesta = await servicio
                .ConsultarDisponibilidadAsync(espacioId, fecha, ct);

            return Results.Ok(respuesta);
        })
        .WithTags("Disponibilidad")
        .WithSummary("Consulta las treinta franjas de un espacio en una fecha")
        .RequireAuthorization()
        .Produces<DisponibilidadResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
