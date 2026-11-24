using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Reservas.Core.Dtos;
using Reservas.Infrastructure.Persistencia;
using Reservas.Tests.Infraestructura;

namespace Reservas.Tests.Pruebas;

public sealed class AutenticacionTests
{
    [Fact]
    public async Task Login_correcto_devuelve_token_y_rol()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient cliente = entorno.Anonimo();

        HttpResponseMessage respuesta = await cliente.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailAdmin, SemillaDesarrollo.ContrasenaAdmin));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        LoginResponse? datos = await respuesta.LeerJsonAsync<LoginResponse>();

        Assert.NotNull(datos);
        Assert.False(string.IsNullOrWhiteSpace(datos!.Token));
        Assert.Equal(SemillaDesarrollo.EmailAdmin, datos.Usuario.Email);
        Assert.Equal("Administrador", datos.Usuario.Rol.ToString());
    }

    [Fact]
    public async Task Login_con_contrasena_incorrecta_devuelve_401()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient cliente = entorno.Anonimo();

        HttpResponseMessage respuesta = await cliente.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailDocente, "contrasena-equivocada"));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_con_correo_inexistente_no_revela_si_el_correo_existe()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient cliente = entorno.Anonimo();

        HttpResponseMessage incorrecta = await cliente.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailDocente, "contrasena-equivocada"));

        HttpResponseMessage inexistente = await cliente.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest("nadie@universidad.edu", "contrasena-equivocada"));

        Assert.Equal(HttpStatusCode.Unauthorized, incorrecta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, inexistente.StatusCode);

        string detalleIncorrecta = (await incorrecta.LeerProblemaAsync()).Detail!;
        string detalleInexistente = (await inexistente.LeerProblemaAsync()).Detail!;
        Assert.Equal(detalleIncorrecta, detalleInexistente);
    }

    [Fact]
    public async Task Usuario_desactivado_no_puede_iniciar_sesion()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient admin = await entorno.ComoAdminAsync();
        await DesactivarAsync(admin, SemillaDesarrollo.EmailEstudiante);

        HttpClient anonimo = entorno.Anonimo();
        HttpResponseMessage respuesta = await anonimo.PostJsonAsync(
            "/api/auth/login",
            new LoginRequest(SemillaDesarrollo.EmailEstudiante, SemillaDesarrollo.ContrasenaEstudiante));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_token_de_un_usuario_desactivado_deja_de_ser_valido()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();
        HttpClient admin = await entorno.ComoAdminAsync();

        HttpResponseMessage antes = await docente.GetAsync("/api/espacios");
        Assert.Equal(HttpStatusCode.OK, antes.StatusCode);

        await DesactivarAsync(admin, SemillaDesarrollo.EmailDocente);

        HttpResponseMessage despues = await docente.GetAsync("/api/espacios");
        Assert.Equal(HttpStatusCode.Unauthorized, despues.StatusCode);
    }

    [Fact]
    public async Task Un_endpoint_protegido_sin_token_devuelve_401()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient anonimo = entorno.Anonimo();

        HttpResponseMessage respuesta = await anonimo.GetAsync("/api/espacios");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_token_alterado_devuelve_401()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient cliente = entorno.Anonimo();
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "esto.no.es.un.token");

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/espacios");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_perfil_devuelve_solo_los_datos_del_usuario_autenticado()
    {
        await using var entorno = AmbienteDePrueba.Crear();
        HttpClient docente = await entorno.ComoDocenteAsync();

        HttpResponseMessage respuesta = await docente.GetAsync("/api/auth/perfil");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        UsuarioResponse? perfil = await respuesta.LeerJsonAsync<UsuarioResponse>();
        Assert.NotNull(perfil);
        Assert.Equal(SemillaDesarrollo.EmailDocente, perfil!.Email);
        Assert.Equal("Docente", perfil.Rol.ToString());

        string crudo = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("hash", crudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("contrasena", crudo, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task DesactivarAsync(HttpClient admin, string email)
    {
        HttpResponseMessage listado = await admin.GetAsync("/api/usuarios");
        List<UsuarioResponse>? usuarios = await listado.LeerJsonAsync<List<UsuarioResponse>>();

        UsuarioResponse destino = usuarios!.Single(u => u.Email == email);

        HttpResponseMessage respuesta = await admin.PutJsonAsync(
            $"/api/usuarios/{destino.Id}",
            new ActualizarUsuarioRequest(destino.Nombre, destino.Rol, false));

        respuesta.EnsureSuccessStatusCode();
    }
}
