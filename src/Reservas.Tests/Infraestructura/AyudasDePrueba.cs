using Reservas.Core.Dtos;

namespace Reservas.Tests.Infraestructura;

public static class AyudasDePrueba
{
    public static DateTime Manana(int hora, int minuto = 0) =>
        DateTime.Today.AddDays(1).AddHours(hora).AddMinutes(minuto);

    public static DateTime EnDosDias(int hora, int minuto = 0) =>
        DateTime.Today.AddDays(2).AddHours(hora).AddMinutes(minuto);

    public static DateTime Ayer(int hora, int minuto = 0) =>
        DateTime.Today.AddDays(-1).AddHours(hora).AddMinutes(minuto);

    public static async Task<int> EspacioAsync(
        this HttpClient cliente,
        string nombre = "Laboratorio de Redes")
    {
        HttpResponseMessage respuesta = await cliente.GetAsync("/api/espacios");
        List<EspacioResponse>? espacios = await respuesta.LeerJsonAsync<List<EspacioResponse>>();

        return espacios!.Single(e => e.Nombre == nombre).Id;
    }

    public static async Task<ReservaResponse> CrearReservaAsync(
        this HttpClient cliente,
        int espacioId,
        DateTime inicio,
        DateTime fin)
    {
        HttpResponseMessage respuesta = await cliente.PostJsonAsync(
            "/api/reservas",
            new CrearReservaRequest(espacioId, inicio, fin));

        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.LeerJsonAsync<ReservaResponse>())!;
    }

    public static async Task<List<ReservaResponse>> ReservasAsync(
        this HttpClient cliente,
        string ruta = "/api/reservas/mias")
    {
        HttpResponseMessage respuesta = await cliente.GetAsync(ruta);
        List<ReservaResponse>? reservas = await respuesta.LeerJsonAsync<List<ReservaResponse>>();

        return reservas!;
    }
}
