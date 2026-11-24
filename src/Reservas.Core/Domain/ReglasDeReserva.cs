namespace Reservas.Core.Domain;

/// <summary>
/// Reglas de horario de una solicitud. Se aíslan en una clase sin dependencias
/// para poder probarlas sin base de datos ni servidor.
/// </summary>
public static class ReglasDeReserva
{
    public const int DuracionMinimaMinutos = 30;

    public static void Validar(DateTime inicio, DateTime fin, DateTime ahora)
    {
        if (fin <= inicio)
            throw new ReglaNegocioException("La hora de fin debe ser posterior a la hora de inicio.");

        if (inicio.Date != fin.Date)
            throw new ReglaNegocioException("La reserva debe comenzar y terminar el mismo día.");

        if (!EsMultiploDeFranja(inicio) || !EsMultiploDeFranja(fin))
            throw new ReglaNegocioException(
                $"Las horas de inicio y fin deben ser múltiplos de {HorarioInstitucional.MinutosPorFranja} minutos.");

        if (inicio.TimeOfDay < HorarioInstitucional.Apertura || fin.TimeOfDay > HorarioInstitucional.Cierre)
            throw new ReglaNegocioException("La reserva debe estar entre las 07:00 y las 22:00.");

        if ((fin - inicio).TotalMinutes < DuracionMinimaMinutos)
            throw new ReglaNegocioException(
                $"La reserva debe durar al menos {DuracionMinimaMinutos} minutos.");

        if (inicio <= ahora)
            throw new ReglaNegocioException("La reserva debe comenzar en el futuro.");
    }

    private static bool EsMultiploDeFranja(DateTime momento) =>
        momento.Second == 0
        && momento.Millisecond == 0
        && (int)momento.TimeOfDay.TotalMinutes % HorarioInstitucional.MinutosPorFranja == 0;
}
