using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class EspaciosTests
{
    [Fact]
    public async Task Un_administrador_crea_un_espacio()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("Laboratorio de Robótica", TipoEspacio.Laboratorio, 25, "Bloque E"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        EspacioResponse? creado = await respuesta.LeerJsonAsync<EspacioResponse>();
        Assert.NotNull(creado);
        Assert.True(creado!.Id > 0);
        Assert.Equal("Laboratorio de Robótica", creado.Nombre);
        Assert.Equal(TipoEspacio.Laboratorio, creado.Tipo);
        Assert.True(creado.Activo);
    }

    [Fact]
    public async Task Un_docente_no_puede_crear_espacios()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("Sala no autorizada", TipoEspacio.Auditorio, 10, "Bloque A"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_estudiante_no_puede_crear_espacios()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();

        HttpResponseMessage respuesta = await estudiante.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("Sala no autorizada", TipoEspacio.Auditorio, 10, "Bloque A"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_catalogo_sembrado_esta_disponible_para_un_estudiante()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient estudiante = await entorno.ComoEstudianteAsync();

        HttpResponseMessage respuesta = await estudiante.GetAsync("/api/espacios");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        List<EspacioResponse>? espacios = await respuesta.LeerJsonAsync<List<EspacioResponse>>();
        Assert.NotNull(espacios);
        Assert.Equal(6, espacios!.Count);
        Assert.All(espacios, e => Assert.True(e.Activo));
    }

    [Fact]
    public async Task El_filtro_por_tipo_devuelve_solo_ese_tipo()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.GetAsync("/api/espacios?tipo=Auditorio");
        List<EspacioResponse>? espacios = await respuesta.LeerJsonAsync<List<EspacioResponse>>();

        Assert.NotNull(espacios);
        Assert.Single(espacios!);
        Assert.Equal(TipoEspacio.Auditorio, espacios[0].Tipo);
    }

    [Fact]
    public async Task El_nombre_vacio_es_rechazado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("   ", TipoEspacio.Laboratorio, 10, "Bloque A"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("nombre", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task La_capacidad_no_positiva_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("Sala vacía", TipoEspacio.SalaDeComputo, 0, "Bloque A"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("capacidad", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task El_nombre_duplicado_se_rechaza_sin_importar_mayusculas()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage primera = await admin.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("Sala de Innovación", TipoEspacio.SalaDeComputo, 20, "Bloque F"));
        primera.EnsureSuccessStatusCode();

        HttpResponseMessage segunda = await admin.PostJsonAsync(
            "/api/espacios",
            new CrearEspacioRequest("sala de innovación", TipoEspacio.SalaDeComputo, 20, "Bloque F"));

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    [Fact]
    public async Task Un_espacio_inexistente_devuelve_404()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.GetAsync("/api/espacios/9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_un_espacio_refleja_los_cambios()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        List<EspacioResponse>? espacios = await (await admin.GetAsync("/api/espacios"))
            .LeerJsonAsync<List<EspacioResponse>>();
        EspacioResponse original = espacios!.First(e => e.Tipo == TipoEspacio.Auditorio);

        HttpResponseMessage respuesta = await admin.PutJsonAsync(
            $"/api/espacios/{original.Id}",
            new ActualizarEspacioRequest(original.Nombre, original.Tipo, 250, "Bloque A, remodelado"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        EspacioResponse? actualizado = await (await admin.GetAsync($"/api/espacios/{original.Id}"))
            .LeerJsonAsync<EspacioResponse>();

        Assert.Equal(250, actualizado!.Capacidad);
        Assert.Equal("Bloque A, remodelado", actualizado.Ubicacion);
    }

    [Fact]
    public async Task Desactivar_un_espacio_lo_mantiene_en_el_catalogo()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        List<EspacioResponse>? espacios = await (await admin.GetAsync("/api/espacios"))
            .LeerJsonAsync<List<EspacioResponse>>();
        EspacioResponse espacio = espacios!.First();

        HttpResponseMessage baja = await admin.DeleteAsync($"/api/espacios/{espacio.Id}");
        Assert.Equal(HttpStatusCode.NoContent, baja.StatusCode);

        EspacioResponse? despues = await (await admin.GetAsync($"/api/espacios/{espacio.Id}"))
            .LeerJsonAsync<EspacioResponse>();
        Assert.NotNull(despues);
        Assert.False(despues!.Activo);

        HttpResponseMessage soloActivos = await admin.GetAsync("/api/espacios?activo=true");
        List<EspacioResponse>? activos = await soloActivos.LeerJsonAsync<List<EspacioResponse>>();
        Assert.DoesNotContain(activos!, e => e.Id == espacio.Id);
    }

    [Fact]
    public async Task Un_tipo_de_espacio_desconocido_es_rechazado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/espacios",
            new { nombre = "Gimnasio", tipo = "Gimnasio", capacidad = 50, ubicacion = "Bloque G" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
