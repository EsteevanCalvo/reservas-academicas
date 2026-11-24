using Reservas.Infrastructure;

namespace Reservas.Api.Inicio;

/// <summary>
/// Aplica migraciones y siembra datos como parte del arranque del host. Se hace
/// aquí y no entre Build y Run para que el servidor no atienda peticiones antes
/// de que el esquema esté listo (importante en las pruebas de integración).
/// </summary>
public sealed class InicializadorDeBaseDeDatos : IHostedService
{
    private readonly IServiceProvider _proveedor;
    private readonly IHostEnvironment _entorno;

    public InicializadorDeBaseDeDatos(IServiceProvider proveedor, IHostEnvironment entorno)
    {
        _proveedor = proveedor;
        _entorno = entorno;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        _proveedor.PrepararBaseDeDatosAsync(_entorno.IsDevelopment(), cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
