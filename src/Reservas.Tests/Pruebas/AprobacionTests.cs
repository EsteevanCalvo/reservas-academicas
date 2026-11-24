using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class AprobacionTests
{
    [Fact]
    public async Task El_administrador_aprueba_una_solicitud()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();

        int espacioId = await docente.EspacioAsync();
        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        UsuarioResponse perfilAdmin = (await (await admin.GetAsync("/api/auth/perfil"))
            .LeerJsonAsync<UsuarioResponse>())!;

        HttpResponseMessage respuesta = await admin.PostAsync(
            $"/api/reservas/{reserva.Id}/aprobar",
            content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        ReservaResponse? aprobada = await respuesta.LeerJsonAsync<ReservaResponse>();
        Assert.Equal(EstadoReserva.Aprobada, aprobada!.Estado);
        Assert.Equal(perfilAdmin.Id, aprobada.DecididaPorId);
        Assert.NotNull(aprobada.DecididaEn);
    }

    [Fact]
    public async Task Un_docente_no_puede_aprobar()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await docente.PostAsync(
            $"/api/reservas/{reserva.Id}/aprobar",
            content: null);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_se_puede_aprobar_dos_veces()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage primera = await admin.PostAsync(
            $"/api/reservas/{reserva.Id}/aprobar", content: null);
        primera.EnsureSuccessStatusCode();

        HttpResponseMessage segunda = await admin.PostAsync(
            $"/api/reservas/{reserva.Id}/aprobar", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
        Assert.Contains("Aprobada", (await segunda.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Aprobar_una_reserva_inexistente_devuelve_404()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostAsync(
            "/api/reservas/9999/aprobar", content: null);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_rechazo_con_motivo_cambia_el_estado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("El laboratorio estará en mantenimiento"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        ReservaResponse? rechazada = await respuesta.LeerJsonAsync<ReservaResponse>();
        Assert.Equal(EstadoReserva.Rechazada, rechazada!.Estado);
        Assert.Equal("El laboratorio estará en mantenimiento", rechazada.MotivoRechazo);
        Assert.NotNull(rechazada.DecididaPorId);
    }

    [Fact]
    public async Task El_rechazo_sin_motivo_es_rechazado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest(null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("motivo", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task El_rechazo_con_motivo_en_blanco_es_rechazado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("    "));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_docente_no_puede_rechazar()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("No corresponde"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_rechazo_libera_la_franja()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage rechazo = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("Sin disponibilidad de equipos"));
        rechazo.EnsureSuccessStatusCode();

        HttpResponseMessage nueva = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9)));

        Assert.Equal(HttpStatusCode.Created, nueva.StatusCode);
    }

    [Fact]
    public async Task No_se_puede_rechazar_una_reserva_cancelada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage cancelacion = await docente.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/cancelar",
            new CancelarReservaRequest("Ya no la necesito"));
        cancelacion.EnsureSuccessStatusCode();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("Motivo tardío"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
