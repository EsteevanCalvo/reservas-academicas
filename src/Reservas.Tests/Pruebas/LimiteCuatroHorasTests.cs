using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

/// <summary>
/// Regla incorporada en la fase 4: ningún usuario puede reservar un espacio
/// durante más de cuatro horas consecutivas. Se prueba la duración de una
/// solicitud y la acumulación de solicitudes contiguas.
/// </summary>
public sealed class LimiteCuatroHorasTests
{
    [Fact]
    public async Task Una_reserva_de_cuatro_horas_se_acepta()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(12)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_reserva_de_cuatro_horas_y_media_se_rechaza()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(12, 30)));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("4 horas", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dos_reservas_contiguas_de_dos_horas_se_aceptan()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage segunda = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12)));

        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
    }

    [Fact]
    public async Task Una_tercera_reserva_contigua_que_supera_el_limite_se_rechaza()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));
        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12));

        HttpResponseMessage tercera = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(12), AyudasDePrueba.Manana(13)));

        Assert.Equal(HttpStatusCode.BadRequest, tercera.StatusCode);
        Assert.Contains("4 horas", (await tercera.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);

        List<ReservaResponse> reservas = await docente.ReservasAsync();
        Assert.Equal(2, reservas.Count);
        Assert.All(reservas, r => Assert.Equal(EstadoReserva.Pendiente, r.Estado));
    }

    [Fact]
    public async Task Un_hueco_reinicia_el_conteo()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));
        await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(10, 30), AyudasDePrueba.Manana(12, 30));

        // El bloque real empieza a las 10:30: 10:30 a 14:30 son cuatro horas.
        HttpResponseMessage tercera = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(12, 30), AyudasDePrueba.Manana(14, 30)));

        Assert.Equal(HttpStatusCode.Created, tercera.StatusCode);
    }

    [Fact]
    public async Task Las_reservas_de_otro_usuario_no_cuentan()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));
        await estudiante.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12));

        // El bloque del docente es solo de una hora: la reserva intermedia es de otro.
        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(12), AyudasDePrueba.Manana(13)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_limite_es_por_espacio()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        int redes = await docente.EspacioAsync("Laboratorio de Redes");
        int fisica = await docente.EspacioAsync("Laboratorio de Física");

        await docente.CrearReservaAsync(redes, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(12));

        // Si el límite fuera global y contiguo, 08:00 a 14:00 serían seis horas.
        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(fisica, AyudasDePrueba.Manana(12), AyudasDePrueba.Manana(14)));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_reserva_cancelada_no_cuenta_para_el_limite()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse segunda = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12));
        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage cancelacion = await docente.PostJsonAsync(
            $"/api/reservas/{segunda.Id}/cancelar",
            new CancelarReservaRequest("Se libera la segunda franja"));
        cancelacion.EnsureSuccessStatusCode();

        // Sin la cancelada, el bloque activo del docente es de 08:00 a 10:00.
        HttpResponseMessage tercera = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(12), AyudasDePrueba.Manana(13)));

        Assert.Equal(HttpStatusCode.Created, tercera.StatusCode);
    }

    [Fact]
    public async Task Una_solicitud_pendiente_cuenta_para_el_limite()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage segunda = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12, 30)));

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
        Assert.Contains("4 horas", (await segunda.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Una_reserva_rechazada_no_ocupa_el_cupo_de_la_cadena()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse primera = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage rechazo = await admin.PostJsonAsync(
            $"/api/reservas/{primera.Id}/rechazar",
            new RechazarReservaRequest("Sin equipos disponibles"));
        rechazo.EnsureSuccessStatusCode();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(10));

        HttpResponseMessage segunda = await docente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(12, 30)));

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }
}
