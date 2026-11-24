namespace Reservas.Api.Seguridad;

public static class Politicas
{
    public const string Administrador = "Administrador";

    /// <summary>
    /// Docentes y estudiantes pueden solicitar reservas. El administrador queda
    /// fuera a propósito: aprobar y solicitar a la vez permitiría la
    /// autoaprobación y eliminaría el control de la operación.
    /// </summary>
    public const string Solicitante = "Solicitante";
}
