using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.Core.Domain;

namespace Reservas.Api.Errores;

/// <summary>
/// Traduce las excepciones de negocio a respuestas HTTP uniformes. El dominio
/// no conoce los códigos de estado; la decisión vive en un solo lugar.
/// </summary>
public sealed class ManejadorDeExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorDeExcepciones> _registro;

    public ManejadorDeExcepciones(ILogger<ManejadorDeExcepciones> registro)
    {
        _registro = registro;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excepcion,
        CancellationToken ct)
    {
        if (contexto.Response.HasStarted)
            return false;

        (int estado, string titulo, string detalle) = excepcion switch
        {
            ReglaNegocioException => (400, "Regla de negocio", excepcion.Message),
            CredencialesInvalidasException => (401, "No autenticado", excepcion.Message),
            AccesoDenegadoException => (403, "Acceso denegado", excepcion.Message),
            NoEncontradoException => (404, "Recurso no encontrado", excepcion.Message),
            ConflictoException => (409, "Conflicto", excepcion.Message),
            DbUpdateException => (409, "Conflicto", "La operación entra en conflicto con datos existentes."),
            // El enlace del cuerpo puede fallar antes de llegar al endpoint
            // (por ejemplo, un enum con un valor que no existe). Eso es un
            // error del cliente, no del servidor.
            BadHttpRequestException solicitud =>
                (solicitud.StatusCode, "Solicitud no válida", "El cuerpo de la petición no es válido."),
            System.Text.Json.JsonException =>
                (400, "Solicitud no válida", "El cuerpo de la petición no es válido."),
            _ => (500, "Error interno", "Ocurrió un error inesperado.")
        };

        if (estado >= 500)
            _registro.LogError(excepcion, "Error no controlado en {Ruta}", contexto.Request.Path);

        contexto.Response.StatusCode = estado;

        await contexto.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = detalle,
                Instance = contexto.Request.Path
            },
            ct);

        return true;
    }
}
