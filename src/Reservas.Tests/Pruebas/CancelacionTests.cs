using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class CancelacionTests
{
    [Fact]
    public async Task El_dueno_cancela_su_reserva()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Cambio de horario de la clase"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        ReservaResponse? cancelada = await respuesta.LeerJsonAsync<ReservaResponse>();
        Assert.Equal(EstadoReserva.Cancelada, cancelada!.Estado);
        Assert.Equal("Cambio de horario de la clase", cancelada.MotivoCancelacion);
        Assert.NotNull(cancelada.CanceladaEn);
    }

    [Fact]
    public async Task La_cancelacion_sin_motivo_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("motivo", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Otro_usuario_no_puede_cancelar_una_reserva_ajena()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("No es mía"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);

        ReservaResponse? sinCambios = await (await docente.GetAsync($"/api/reservas/{reserva.Id}"))
            .LeerJsonAsync<ReservaResponse>();
        Assert.Equal(EstadoReserva.Pendiente, sinCambios!.Estado);
    }

    [Fact]
    public async Task El_administrador_puede_cancelar_la_reserva_de_otro()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Cierre del edificio"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        ReservaResponse? cancelada = await respuesta.LeerJsonAsync<ReservaResponse>();
        Assert.Equal(EstadoReserva.Cancelada, cancelada!.Estado);
    }

    [Fact]
    public async Task No_se_puede_cancelar_una_reserva_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage rechazo = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("No cumple el objetivo académico"));
        rechazo.EnsureSuccessStatusCode();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Intento tardío"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("Rechazada", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_se_puede_cancelar_dos_veces()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage primera = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Primera cancelación"));
        primera.EnsureSuccessStatusCode();

        HttpResponseMessage segunda = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Segunda cancelación"));

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task La_cancelacion_libera_la_franja()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage cancelacion = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Se suspendió la práctica"));
        cancelacion.EnsureSuccessStatusCode();

        HttpResponseMessage nueva = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Created, nueva.StatusCode);
    }
}
