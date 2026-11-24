namespace Reservas.Core.Domain;

/// <summary>
/// Horario de atención de la institución. Se asume una única zona horaria y
/// una cuadrícula fija de 30 minutos, que es lo que permite convertir la
/// prevención de solapamientos en un índice único.
/// </summary>
public static class HorarioInstitucional
{
    public static readonly TimeSpan Apertura = TimeSpan.FromHours(7);
    public static readonly TimeSpan Cierre = TimeSpan.FromHours(22);

    public const int MinutosPorFranja = 30;
    public const int TotalFranjas = 30;

    public static int FranjaDe(DateTime momento) =>
        (int)((momento.TimeOfDay - Apertura).TotalMinutes / MinutosPorFranja);

    public static DateTime InicioDeFranja(DateOnly fecha, int indice) =>
        fecha.ToDateTime(TimeOnly.MinValue).Add(Apertura).AddMinutes(indice * MinutosPorFranja);

    public static DateTime FinDeFranja(DateOnly fecha, int indice) =>
        InicioDeFranja(fecha, indice).AddMinutes(MinutosPorFranja);

    public static IReadOnlyList<int> FranjasEntre(DateTime inicio, DateTime fin)
    {
        int primera = FranjaDe(inicio);
        int ultima = FranjaDe(fin) - 1;

        var franjas = new List<int>(ultima - primera + 1);

        for (int indice = primera; indice <= ultima; indice++)
            franjas.Add(indice);

        return franjas;
    }
}
