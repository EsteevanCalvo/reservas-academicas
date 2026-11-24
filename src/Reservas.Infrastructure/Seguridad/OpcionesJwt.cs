using System.Text;

namespace Reservas.Infrastructure.Seguridad;

public sealed class OpcionesJwt
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = string.Empty;

    public string Emisor { get; set; } = "ReservasAcademicas";

    public string Audiencia { get; set; } = "ReservasAcademicas";

    public int HorasValidez { get; set; } = 8;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Clave))
            throw new InvalidOperationException("Falta la clave de firma de tokens (Jwt:Clave).");

        if (Encoding.UTF8.GetByteCount(Clave) < 32)
            throw new InvalidOperationException("La clave de firma de tokens debe tener al menos 32 bytes.");

        if (string.IsNullOrWhiteSpace(Emisor) || string.IsNullOrWhiteSpace(Audiencia))
            throw new InvalidOperationException("El emisor y la audiencia de los tokens son obligatorios.");

        if (HorasValidez <= 0)
            throw new InvalidOperationException("La validez del token debe ser mayor que cero.");
    }
}
