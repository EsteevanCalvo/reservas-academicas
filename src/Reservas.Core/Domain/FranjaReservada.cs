namespace Reservas.Core.Domain;

public sealed class FranjaReservada
{
    public int Id { get; private set; }

    public int ReservaId { get; private set; }

    public int EspacioId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int Indice { get; private set; }

    private FranjaReservada()
    {
        // Constructor requerido por EF Core.
    }

    public FranjaReservada(int espacioId, DateOnly fecha, int indice)
    {
        EspacioId = espacioId;
        Fecha = fecha;
        Indice = indice;
    }
}
