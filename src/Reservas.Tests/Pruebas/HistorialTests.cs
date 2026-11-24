using System.Globalization;
using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class HistorialTests
{
    [Fact]
    public async Task Un_docente_solo_ve_sus_reservas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();

        int redes = await docente.EspacioAsync("Laboratorio de Redes");
        int fisica = await docente.EspacioAsync("Laboratorio de Física");

        await docente.CrearReservaAsync(redes, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        await estudiante.CrearReservaAsync(fisica, AyudasDePrueba.Manana(9), AyudasDePrueba.Manana(10));

        List<ReservaResponse> delDocente = await docente.ReservasAsync();
        List<ReservaResponse> delEstudiante = await estudiante.ReservasAsync();

        Assert.Single(delDocente);
        Assert.Single(delEstudiante);
        Assert.Equal("docente@universidad.edu", delDocente[0].EmailUsuario);
        Assert.Equal("estudiante@universidad.edu", delEstudiante[0].EmailUsuario);
    }

    [Fact]
    public async Task El_administrador_ve_todas_las_reservas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();

        int redes = await docente.EspacioAsync("Laboratorio de Redes");
        int fisica = await docente.EspacioAsync("Laboratorio de Física");

        await docente.CrearReservaAsync(redes, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        await estudiante.CrearReservaAsync(fisica, AyudasDePrueba.Manana(9), AyudasDePrueba.Manana(10));

        List<ReservaResponse> todas = await admin.ReservasAsync("/api/reservas");

        Assert.Equal(2, todas.Count);
    }

    [Fact]
    public async Task Un_docente_no_puede_ver_el_historial_global()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.GetAsync("/api/reservas");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_filtro_por_estado_funciona()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse primera = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(10), AyudasDePrueba.Manana(11));

        HttpResponseMessage cancelacion = await docente.PostJsonAsync(
            $"/api/reservas/{primera.Id}/cancelar",
            new CancelarReservaRequest("Se canceló la clase"));
        cancelacion.EnsureSuccessStatusCode();

        List<ReservaResponse> canceladas = await docente.ReservasAsync("/api/reservas/mias?estado=Cancelada");

        Assert.Single(canceladas);
        Assert.Equal(primera.Id, canceladas[0].Id);
    }

    [Fact]
    public async Task El_filtro_por_espacio_funciona()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        int redes = await docente.EspacioAsync("Laboratorio de Redes");
        int fisica = await docente.EspacioAsync("Laboratorio de Física");

        await docente.CrearReservaAsync(redes, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        await docente.CrearReservaAsync(fisica, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        List<ReservaResponse> soloRedes = await docente.ReservasAsync(
            $"/api/reservas/mias?espacioId={redes}");

        Assert.Single(soloRedes);
        Assert.Equal(redes, soloRedes[0].EspacioId);
    }

    [Fact]
    public async Task El_filtro_por_fechas_funciona()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.EnDosDias(8), AyudasDePrueba.EnDosDias(9));

        string desde = Uri.EscapeDataString(
            AyudasDePrueba.Manana(0).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        string hasta = Uri.EscapeDataString(
            AyudasDePrueba.Manana(23, 59).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));

        List<ReservaResponse> manana = await docente.ReservasAsync(
            $"/api/reservas/mias?desde={desde}&hasta={hasta}");

        Assert.Single(manana);
        Assert.Equal(AyudasDePrueba.Manana(8), manana[0].Inicio);
    }

    [Fact]
    public async Task Consultar_una_reserva_ajena_por_id_devuelve_403()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await estudiante.GetAsync($"/api/reservas/{reserva.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task La_respuesta_incluye_el_motivo_y_el_responsable()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();
        int espacioId = await docente.EspacioAsync();

        ReservaResponse reserva = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage rechazo = await admin.PostJsonAsync(
            $"/api/reservas/{reserva.Id}/rechazar",
            new RechazarReservaRequest("Falta de equipos"));
        rechazo.EnsureSuccessStatusCode();

        UsuarioResponse perfilAdmin = (await (await admin.GetAsync("/api/auth/perfil"))
            .LeerJsonAsync<UsuarioResponse>())!;

        ReservaResponse? detalle = await (await admin.GetAsync($"/api/reservas/{reserva.Id}"))
            .LeerJsonAsync<ReservaResponse>();

        Assert.Equal("Falta de equipos", detalle!.MotivoRechazo);
        Assert.Equal(perfilAdmin.Id, detalle.DecididaPorId);
        Assert.NotNull(detalle.DecididaEn);
    }

    [Fact]
    public async Task El_historial_se_ordena_por_inicio_descendente()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));
        ReservaResponse segunda = await docente.CrearReservaAsync(
            espacioId, AyudasDePrueba.EnDosDias(8), AyudasDePrueba.EnDosDias(9));

        List<ReservaResponse> reservas = await docente.ReservasAsync();

        Assert.Equal(2, reservas.Count);
        Assert.Equal(segunda.Id, reservas[0].Id);
    }
}
