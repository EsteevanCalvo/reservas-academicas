using System.Net;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class ConcurrenciaTests
{
    [Fact]
    public async Task Diez_solicitudes_simultaneas_producen_una_sola_reserva()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        int espacioId = await docente.EspacioAsync();

        var peticion = new CrearReservaRequest(
            espacioId,
            AyudasDePrueba.Manana(8),
            AyudasDePrueba.Manana(10));

        Task<HttpResponseMessage>[] tareas = Enumerable
            .Range(0, 10)
            .Select(_ => docente.PostJsonAsync("/api/reservas", peticion))
            .ToArray();

        HttpResponseMessage[] respuestas = await Task.WhenAll(tareas);

        Assert.Equal(1, respuestas.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(9, respuestas.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        List<ReservaResponse> reservas = await docente.ReservasAsync();
        Assert.Single(reservas);
    }
}
