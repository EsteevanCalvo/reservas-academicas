using System.Net;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;
using Reservas.Infrastructure.Persistencia;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class UsuariosTests
{
    [Fact]
    public async Task Un_administrador_registra_un_usuario()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/usuarios",
            new CrearUsuarioRequest("Carlos Ruiz", "carlos.ruiz@universidad.edu", "Clave123!", Rol.Docente));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        UsuarioResponse? creado = await respuesta.LeerJsonAsync<UsuarioResponse>();
        Assert.NotNull(creado);
        Assert.Equal("carlos.ruiz@universidad.edu", creado!.Email);
        Assert.True(creado.Activo);
    }

    [Fact]
    public async Task El_correo_duplicado_devuelve_conflicto()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/usuarios",
            new CrearUsuarioRequest("Otra persona", "docente@universidad.edu", "Clave123!", Rol.Docente));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Una_contrasena_corta_es_rechazada()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/usuarios",
            new CrearUsuarioRequest("Carla Méndez", "carla@universidad.edu", "corta", Rol.Estudiante));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("8", (await respuesta.LeerProblemaAsync()).Detail!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task El_listado_no_expone_contrasenas_ni_hashes()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.GetAsync("/api/usuarios");
        string crudo = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.DoesNotContain("hash", crudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("contrasena", crudo, StringComparison.OrdinalIgnoreCase);

        List<UsuarioResponse>? usuarios = await respuesta.LeerJsonAsync<List<UsuarioResponse>>();
        Assert.Equal(3, usuarios!.Count);
    }

    [Fact]
    public async Task Un_docente_no_puede_listar_usuarios()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Cambiar_el_rol_otorga_los_permisos_correspondientes()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        List<UsuarioResponse>? usuarios = await (await admin.GetAsync("/api/usuarios"))
            .LeerJsonAsync<List<UsuarioResponse>>();
        UsuarioResponse estudiante = usuarios!.Single(u => u.Email == SemillaDesarrollo.EmailEstudiante);

        HttpResponseMessage cambio = await admin.PutJsonAsync(
            $"/api/usuarios/{estudiante.Id}",
            new ActualizarUsuarioRequest(estudiante.Nombre, Rol.Administrador, true));
        Assert.Equal(HttpStatusCode.OK, cambio.StatusCode);

        HttpClient anonimo = entorno.Anonimo();
        HttpResponseMessage login = await anonimo.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailEstudiante, SemillaDesarrollo.ContrasenaEstudiante));
        LoginResponse? datos = await login.LeerJsonAsync<LoginResponse>();

        anonimo.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", datos!.Token);

        HttpResponseMessage listado = await anonimo.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.OK, listado.StatusCode);
    }

    [Fact]
    public async Task Restablecer_la_contrasena_cambia_el_acceso()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        List<UsuarioResponse>? usuarios = await (await admin.GetAsync("/api/usuarios"))
            .LeerJsonAsync<List<UsuarioResponse>>();
        UsuarioResponse docente = usuarios!.Single(u => u.Email == SemillaDesarrollo.EmailDocente);

        HttpResponseMessage cambio = await admin.PostJsonAsync(
            $"/api/usuarios/{docente.Id}/contrasena",
            new RestablecerContrasenaRequest("NuevaClave123!"));
        Assert.Equal(HttpStatusCode.NoContent, cambio.StatusCode);

        HttpClient anonimo = entorno.Anonimo();

        HttpResponseMessage anterior = await anonimo.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailDocente, SemillaDesarrollo.ContrasenaDocente));
        Assert.Equal(HttpStatusCode.Unauthorized, anterior.StatusCode);

        HttpResponseMessage nueva = await anonimo.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailDocente, "NuevaClave123!"));
        Assert.Equal(HttpStatusCode.OK, nueva.StatusCode);
    }

    [Fact]
    public async Task El_nombre_vacio_de_un_usuario_es_rechazado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage respuesta = await admin.PostJsonAsync(
            "/api/usuarios",
            new CrearUsuarioRequest("  ", "sin.nombre@universidad.edu", "Clave123!", Rol.Estudiante));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
