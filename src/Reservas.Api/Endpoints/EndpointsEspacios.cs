using Reservas.Api.Seguridad;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Core.Servicios;

namespace Reservas.Api.Endpoints;

public static class EndpointsEspacios
{
    public static void Mapear(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder grupo = app.MapGroup("/api/espacios")
            .WithTags("Espacios")
            .RequireAuthorization();

        grupo.MapGet("/", async (
            TipoEspacio? tipo,
            bool? activo,
            ServicioEspacios servicio,
            CancellationToken ct) =>
        {
            IReadOnlyList<Espacio> espacios = await servicio.ListarAsync(tipo, activo, ct);
            return Results.Ok(espacios.Select(ARespuesta));
        })
        .WithSummary("Lista los espacios académicos, filtrable por tipo y estado")
        .Produces<IEnumerable<EspacioResponse>>();

        grupo.MapGet("/{id:int}", async (
            int id,
            ServicioEspacios servicio,
            CancellationToken ct) =>
        {
            Espacio espacio = await servicio.ObtenerAsync(id, ct);
            return Results.Ok(ARespuesta(espacio));
        })
        .WithSummary("Obtiene un espacio por identificador")
        .Produces<EspacioResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async (
            CrearEspacioRequest peticion,
            ServicioEspacios servicio,
            CancellationToken ct) =>
        {
            Espacio espacio = await servicio.CrearAsync(peticion, ct);

            return Results.Created(
                $"/api/espacios/{espacio.Id}",
                ARespuesta(espacio));
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Registra un espacio académico (solo Administrador)")
        .Produces<EspacioResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:int}", async (
            int id,
            ActualizarEspacioRequest peticion,
            ServicioEspacios servicio,
            CancellationToken ct) =>
        {
            Espacio espacio = await servicio.ActualizarAsync(id, peticion, ct);
            return Results.Ok(ARespuesta(espacio));
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Actualiza un espacio (solo Administrador)")
        .Produces<EspacioResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}", async (
            int id,
            ServicioEspacios servicio,
            CancellationToken ct) =>
        {
            await servicio.DesactivarAsync(id, ct);
            return Results.NoContent();
        })
        .RequireAuthorization(Politicas.Administrador)
        .WithSummary("Desactiva un espacio sin borrar su historial (solo Administrador)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static EspacioResponse ARespuesta(Espacio espacio) =>
        new(
            espacio.Id,
            espacio.Nombre,
            espacio.Tipo,
            espacio.Capacidad,
            espacio.Ubicacion,
            espacio.Activo);
}
