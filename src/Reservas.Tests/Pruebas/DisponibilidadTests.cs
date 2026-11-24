using System.Globalization;
using System.Net;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class DisponibilidadTests
{
    private static string Ruta(int espacioId, DateOnly fecha) =>
        $"/api/disponibilidad?espacioId={espacioId}&fecha={fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

    [Fact]
    public async Task Devuelve_treinta_franjas_desde_las_siete_hasta_las_veintidos()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();
        DateOnly fecha = DateOnly.FromDateTime(AyudasDePrueba.Manana(0));

        HttpResponseMessage respuesta = await docente.GetAsync(Ruta(espacioId, fecha));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        DisponibilidadResponse? disponibilidad = await respuesta.LeerJsonAsync<DisponibilidadResponse>();
        Assert.NotNull(disponibilidad);
        Assert.Equal(30, disponibilidad!.Franjas.Count);
        Assert.Equal(AyudasDePrueba.Manana(7), disponibilidad.Franjas[0].Inicio);
        Assert.Equal(AyudasDePrueba.Manana(21, 30), disponibilidad.Franjas[^1].Inicio);
        Assert.Equal(AyudasDePrueba.Manana(22), disponibilidad.Franjas[^1].Fin);
        Assert.All(disponibilidad.Franjas, f => Assert.True(f.Disponible));
    }

    [Fact]
    public async Task Una_reserva_ocupa_sus_franjas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();
        DateOnly fecha = DateOnly.FromDateTime(AyudasDePrueba.Manana(0));

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        DisponibilidadResponse? disponibilidad = await (await docente.GetAsync(Ruta(espacioId, fecha)))
            .LeerJsonAsync<DisponibilidadResponse>();

        Assert.False(disponibilidad!.Franjas[2].Disponible);
        Assert.False(disponibilidad.Franjas[3].Disponible);
        Assert.True(disponibilidad.Franjas[4].Disponible);
        Assert.True(disponibilidad.Franjas[0].Disponible);
    }

    [Fact]
    public async Task Un_espacio_inexistente_devuelve_404()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.GetAsync(
            Ruta(9999, DateOnly.FromDateTime(AyudasDePrueba.Manana(0))));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Las_franjas_que_ya_pasaron_no_figuran_disponibles()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        DateTime antes = DateTime.Now;
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

        DisponibilidadResponse? disponibilidad = await (await docente.GetAsync(Ruta(espacioId, hoy)))
            .LeerJsonAsync<DisponibilidadResponse>();

        Assert.All(
            disponibilidad!.Franjas.Where(f => f.Inicio <= antes),
            f => Assert.False(f.Disponible));
    }

    [Fact]
    public async Task La_disponibilidad_no_expone_datos_de_otros_usuarios()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();
        DateOnly fecha = DateOnly.FromDateTime(AyudasDePrueba.Manana(0));

        await docente.CrearReservaAsync(espacioId, AyudasDePrueba.Manana(8), AyudasDePrueba.Manana(9));

        HttpResponseMessage respuesta = await docente.GetAsync(Ruta(espacioId, fecha));
        string crudo = await respuesta.Content.ReadAsStringAsync();

        Assert.DoesNotContain("docente@universidad.edu", crudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ana Torres", crudo, StringComparison.OrdinalIgnoreCase);
    }
}
