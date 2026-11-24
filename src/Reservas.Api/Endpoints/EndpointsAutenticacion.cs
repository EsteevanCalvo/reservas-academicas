using Reservas.Api.Seguridad;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Core.Servicios;

namespace Reservas.Api.Endpoints;

public static class EndpointsAutenticacion
{
    public static void Mapear(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder grupo = app.MapGroup("/api/auth").WithTags("Autenticación");

        grupo.MapPost("/login", async (
            LoginRequest peticion,
            ServicioAutenticacion servicio,
            CancellationToken ct) =>
        {
            LoginResponse respuesta = await servicio.IniciarSesionAsync(peticion, ct);
            return Results.Ok(respuesta);
        })
        .AllowAnonymous()
        .WithSummary("Inicia sesión y devuelve un token JWT")
        .Produces<LoginResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/perfil", (HttpContext contexto) =>
        {
            Usuario usuario = contexto.Actual();

            return Results.Ok(new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol,
                usuario.Activo));
        })
        .RequireAuthorization()
        .WithSummary("Devuelve el perfil del usuario autenticado")
        .Produces<UsuarioResponse>();
    }
}
