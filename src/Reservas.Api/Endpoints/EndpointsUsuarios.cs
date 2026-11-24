using Reservas.Api.Seguridad;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Core.Servicios;

namespace Reservas.Api.Endpoints;

public static class EndpointsUsuarios
{
    public static void Mapear(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(Politicas.Administrador);

        grupo.MapGet("/", async (ServicioUsuarios servicio, CancellationToken ct) =>
        {
            IReadOnlyList<Usuario> usuarios = await servicio.ListarAsync(ct);
            return Results.Ok(usuarios.Select(ARespuesta));
        })
        .WithSummary("Lista los usuarios registrados")
        .Produces<IEnumerable<UsuarioResponse>>();

        grupo.MapPost("/", async (
            CrearUsuarioRequest peticion,
            ServicioUsuarios servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = await servicio.CrearAsync(peticion, ct);

            return Results.Created(
                $"/api/usuarios/{usuario.Id}",
                ARespuesta(usuario));
        })
        .WithSummary("Registra un usuario")
        .Produces<UsuarioResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:int}", async (
            int id,
            ActualizarUsuarioRequest peticion,
            ServicioUsuarios servicio,
            CancellationToken ct) =>
        {
            Usuario usuario = await servicio.ActualizarAsync(id, peticion, ct);
            return Results.Ok(ARespuesta(usuario));
        })
        .WithSummary("Actualiza nombre, rol y estado de un usuario")
        .Produces<UsuarioResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/{id:int}/contrasena", async (
            int id,
            RestablecerContrasenaRequest peticion,
            ServicioUsuarios servicio,
            CancellationToken ct) =>
        {
            await servicio.RestablecerContrasenaAsync(id, peticion, ct);
            return Results.NoContent();
        })
        .WithSummary("Restablece la contraseña de un usuario")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static UsuarioResponse ARespuesta(Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol,
            usuario.Activo);
}
