using Reservas.Core.Abstractions;
using Reservas.Core.Domain;
using Reservas.Core.Dtos;

namespace Reservas.Core.Servicios;

public sealed class ServicioEspacios
{
    private readonly IRepositorioEspacios _espacios;
    private readonly IUnidadDeTrabajo _unidad;

    public ServicioEspacios(IRepositorioEspacios espacios, IUnidadDeTrabajo unidad)
    {
        _espacios = espacios;
        _unidad = unidad;
    }

    public Task<IReadOnlyList<Espacio>> ListarAsync(TipoEspacio? tipo, bool? activo, CancellationToken ct) =>
        _espacios.ListarAsync(tipo, activo, ct);

    public async Task<Espacio> ObtenerAsync(int id, CancellationToken ct)
    {
        Espacio? espacio = await _espacios.ObtenerAsync(id, ct);

        if (espacio is null)
            throw new NoEncontradoException($"No se encontró el espacio con identificador {id}.");

        return espacio;
    }

    public async Task<Espacio> CrearAsync(CrearEspacioRequest peticion, CancellationToken ct)
    {
        ValidarTipo(peticion.Tipo);
        string nombre = (peticion.Nombre ?? string.Empty).Trim();

        if (await _espacios.ExisteNombreAsync(nombre, null, ct))
            throw new ConflictoException($"Ya existe un espacio llamado '{nombre}'.");

        var espacio = new Espacio(nombre, peticion.Tipo, peticion.Capacidad, peticion.Ubicacion);
        _espacios.Agregar(espacio);
        await _unidad.GuardarAsync(ct);
        return espacio;
    }

    public async Task<Espacio> ActualizarAsync(int id, ActualizarEspacioRequest peticion, CancellationToken ct)
    {
        ValidarTipo(peticion.Tipo);
        Espacio espacio = await ObtenerAsync(id, ct);
        string nombre = (peticion.Nombre ?? string.Empty).Trim();

        if (await _espacios.ExisteNombreAsync(nombre, id, ct))
            throw new ConflictoException($"Ya existe un espacio llamado '{nombre}'.");

        espacio.Actualizar(nombre, peticion.Tipo, peticion.Capacidad, peticion.Ubicacion);
        await _unidad.GuardarAsync(ct);
        return espacio;
    }

    public async Task DesactivarAsync(int id, CancellationToken ct)
    {
        Espacio espacio = await ObtenerAsync(id, ct);
        espacio.Desactivar();
        await _unidad.GuardarAsync(ct);
    }

    private static void ValidarTipo(TipoEspacio tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new ReglaNegocioException("El tipo de espacio no es válido.");
    }
}
