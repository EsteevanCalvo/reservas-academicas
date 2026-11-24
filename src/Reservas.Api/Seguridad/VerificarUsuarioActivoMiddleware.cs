using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Api.Seguridad;

/// <summary>
/// Relee el usuario en cada petición autenticada. Un token válido no basta:
/// el usuario debe seguir existiendo y estar activo. Es el precio de no
/// mantener una lista negra de tokens y lo que permite revocar el acceso.
/// </summary>
public sealed class VerificarUsuarioActivoMiddleware
{
    private readonly RequestDelegate _siguiente;

    public VerificarUsuarioActivoMiddleware(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto, IRepositorioUsuarios usuarios)
    {
        if (contexto.User.Identity?.IsAuthenticated == true)
        {
            string? idTexto = contexto.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(idTexto, out int id))
            {
                await RechazarAsync(contexto);
                return;
            }

            Usuario? usuario = await usuarios.ObtenerAsync(id, contexto.RequestAborted);

            if (usuario is null || !usuario.Activo)
            {
                await RechazarAsync(contexto);
                return;
            }

            contexto.Items[ContextoUsuario.Clave] = usuario;
        }

        await _siguiente(contexto);
    }

    private static async Task RechazarAsync(HttpContext contexto)
    {
        contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await contexto.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status401Unauthorized,
            title = "No autenticado",
            detail = "La sesión no es válida o el usuario está desactivado."
        });
    }
}

public static class ContextoUsuario
{
    public const string Clave = "usuario";

    public static Usuario Actual(this HttpContext contexto) =>
        contexto.Items[Clave] as Usuario
        ?? throw new InvalidOperationException("La petición no tiene un usuario autenticado en el contexto.");
}
