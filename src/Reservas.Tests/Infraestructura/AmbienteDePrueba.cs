using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reservas.Core.Dtos;
using Reservas.Infrastructure.Persistencia;

namespace Reservas.Tests.Infraestructura;

/// <summary>
/// Cada prueba levanta su propia API contra un archivo SQLite temporal. Así se
/// ejerce el código real (middleware, autenticación, EF Core, restricciones de
/// la base) y ninguna prueba depende del estado que dejó otra.
/// </summary>
public sealed class AmbienteDePrueba : IAsyncDisposable
{
    private readonly string _rutaBase;

    private AmbienteDePrueba(string rutaBase)
    {
        _rutaBase = rutaBase;

        Fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(constructor =>
        {
            constructor.UseEnvironment("Development");
            constructor.ConfigureAppConfiguration((_, configuracion) =>
            {
                configuracion.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={_rutaBase};Default Timeout=30"
                });
            });

            // Refuerzo: aunque la configuración se aplique en otro orden, el
            // contexto de la prueba apunta siempre al archivo temporal.
            constructor.ConfigureTestServices(servicios =>
            {
                servicios.RemoveAll<DbContextOptions<ReservasDbContext>>();
                servicios.AddDbContext<ReservasDbContext>(opciones =>
                    opciones.UseSqlite($"Data Source={_rutaBase};Default Timeout=30"));
            });
        });
    }

    public WebApplicationFactory<Program> Fabrica { get; }

    public static AmbienteDePrueba Crear()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"reservas-prueba-{Guid.NewGuid():N}.db");
        var ambiente = new AmbienteDePrueba(ruta);

        // Arranca el host: aplica migraciones y siembra los datos de desarrollo.
        _ = ambiente.Fabrica.Server;

        return ambiente;
    }

    public HttpClient Anonimo() => Fabrica.CreateClient();

    public Task<HttpClient> ComoAdminAsync() =>
        CrearClienteAsync(SemillaDesarrollo.EmailAdmin, SemillaDesarrollo.ContrasenaAdmin);

    public Task<HttpClient> ComoDocenteAsync() =>
        CrearClienteAsync(SemillaDesarrollo.EmailDocente, SemillaDesarrollo.ContrasenaDocente);

    public Task<HttpClient> ComoEstudianteAsync() =>
        CrearClienteAsync(SemillaDesarrollo.EmailEstudiante, SemillaDesarrollo.ContrasenaEstudiante);

    private async Task<HttpClient> CrearClienteAsync(string email, string contrasena)
    {
        HttpClient cliente = Fabrica.CreateClient();

        HttpResponseMessage respuesta = await cliente.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, contrasena));

        respuesta.EnsureSuccessStatusCode();

        LoginResponse? datos = await respuesta.LeerJsonAsync<LoginResponse>();
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", datos!.Token);

        return cliente;
    }

    public async ValueTask DisposeAsync()
    {
        await Fabrica.DisposeAsync();

        foreach (string sufijo in new[] { string.Empty, "-shm", "-wal" })
        {
            string ruta = _rutaBase + sufijo;

            try
            {
                if (File.Exists(ruta))
                    File.Delete(ruta);
            }
            catch (IOException)
            {
                // Un archivo temporal que no se puede borrar no invalida la prueba.
            }
        }
    }
}

public static class JsonDePrueba
{
    /// <summary>
    /// El servidor escribe los enums como texto; el cliente de pruebas debe leer
    /// con la misma convención o la deserialización falla.
    /// </summary>
    public static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static Task<HttpResponseMessage> PostJsonAsync<T>(
        this HttpClient cliente,
        string ruta,
        T valor) => cliente.PostAsJsonAsync(ruta, valor, Opciones);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(
        this HttpClient cliente,
        string ruta,
        T valor) => cliente.PutAsJsonAsync(ruta, valor, Opciones);

    public static async Task<T?> LeerJsonAsync<T>(this HttpResponseMessage respuesta) =>
        await respuesta.Content.ReadFromJsonAsync<T>(Opciones);

    public static async Task<ProblemDetails> LeerProblemaAsync(this HttpResponseMessage respuesta) =>
        await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(Opciones)
        ?? throw new InvalidOperationException("La respuesta no contiene un ProblemDetails.");
}
