using System.Net;
using System.Net.Http.Json;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class ReservasTests
{
    [Fact]
    public async Task Un_docente_crea_una_solicitud_pendiente()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        ReservaResponse? reserva = await respuesta.LeerJsonAsync<ReservaResponse>();
        Assert.NotNull(reserva);
        Assert.Equal(EstadoReserva.Pendiente, reserva!.Estado);
        Assert.Equal("Laboratorio de Redes", reserva.Espacio);
        Assert.Equal("Ana Torres", reserva.Usuario);
        Assert.True(reserva.Id > 0);
    }

    [Fact]
    public async Task Un_administrador_no_puede_crear_solicitudes()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await admin.EspacioAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Sin_token_no_se_puede_crear_una_solicitud()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient anonimo = entorno.Anonimo();

        HttpResponseMessage respuesta = await anonimo.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(1, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_reserva_fuera_del_horario_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(6), AyudasDePrueba.Manana(8)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("07:00", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Una_reserva_desalineada_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(
                espacioId,
                AyudasDePrueba.Manana(8, 15),
                AyudasDePrueba.Manana(9, 15)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_reserva_en_el_pasado_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Ayer(8), AyudasDePrueba.Ayer(10)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("futuro", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Un_espacio_inexistente_devuelve_404()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(9999, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9)));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_espacio_desactivado_no_admite_reservas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage baja = await admin.DeleteAsync($"/api/espacios/{espacioId}");
        baja.EnsureSuccessStatusCode();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("desactivado", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task El_solapamiento_exacto_devuelve_conflicto()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_solapamiento_parcial_devuelve_conflicto()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(9), AyudasDePrueba.Manana(11)));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_reserva_que_rodea_a_otra_devuelve_conflicto()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(9), AyudasDePrueba.Manana(10));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(11)));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Las_franjas_contiguas_estan_permitidas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(9), AyudasDePrueba.Manana(10)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task Dos_espacios_admiten_la_misma_franja()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();

        int redes = await docente.EspacioAsync("Laboratorio de Redes");
        int fisica = await docente.EspacioAsync("Laboratorio de Física");

        await docente.CrearReservaAsync(redes, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(fisica, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }
}
