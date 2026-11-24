namespace Reservas.Core.Domain;

/// <summary>
/// Reglas de horario de una solicitud. Se aíslan en una clase sin dependencias
/// para poder probarlas sin base de datos ni servidor.
/// </summary>
public static class ReglasDeReserva
{
    public const int DuracionMinimaMinutos = 30;

    /// <summary>
    /// Regla incorporada después de la primera entrega: ningún usuario puede
    /// reservar un espacio durante más de cuatro horas consecutivas.
    /// </summary>
    public const int MaximoHorasConsecutivas = 4;

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

        if ((fin - inicio).TotalHours > MaximoHorasConsecutivas)
            throw new ReglaNegocioException(
                $"Una reserva no puede superar las {MaximoHorasConsecutivas} horas consecutivas.");

        if (inicio <= ahora)
            throw new ReglaNegocioException("La reserva debe comenzar en el futuro.");
    }

    /// <summary>
    /// Comprueba la acumulación: calcula el bloque continuo de reservas activas
    /// del mismo usuario y espacio que toca o solapa a la nueva solicitud y
    /// rechaza si ese bloque supera el máximo. Un hueco corta la cadena.
    /// </summary>
    public static void ValidarLimiteConsecutivo(
        IEnumerable<(DateTime Inicio, DateTime Fin)> reservasActivas,
        DateTime inicio,
        DateTime fin)
    {
        DateTime bloqueInicio = inicio;
        DateTime bloqueFin = fin;

        List<(DateTime Inicio, DateTime Fin)> reservas = reservasActivas.ToList();
        bool extendido = true;

        // El bloque puede crecer por ambos extremos y en cualquier orden, así
        // que se repite hasta que ninguna reserva lo amplíe.
        while (extendido)
        {
            extendido = false;

            foreach ((DateTime inicioReserva, DateTime finReserva) in reservas)
            {
                // Se tocan (fin == inicio) o se superponen: forman un solo bloque.
                if (finReserva < bloqueInicio || inicioReserva > bloqueFin)
                    continue;

                if (inicioReserva < bloqueInicio)
                {
                    bloqueInicio = inicioReserva;
                    extendido = true;
                }

                if (finReserva > bloqueFin)
                {
                    bloqueFin = finReserva;
                    extendido = true;
                }
            }
        }

        if ((bloqueFin - bloqueInicio).TotalHours > MaximoHorasConsecutivas)
            throw new ReglaNegocioException(
                $"Un usuario no puede reservar el mismo espacio durante más de {MaximoHorasConsecutivas} horas consecutivas.");
    }

    private static bool EsMultiploDeFranja(DateTime momento) =>
        momento.Second == 0
        && momento.Millisecond == 0
        && (int)momento.TimeOfDay.TotalMinutes % HorarioInstitucional.MinutosPorFranja == 0;
}
