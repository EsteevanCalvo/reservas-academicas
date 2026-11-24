using System.Security.Cryptography;
using Reservas.Core.Abstractions;

namespace Reservas.Infrastructure.Seguridad;

/// <summary>
/// PBKDF2 con SHA-256. El formato guardado incluye el algoritmo y el número de
/// iteraciones, de modo que se pueden subir en el futuro sin invalidar los
/// hashes existentes: al verificar se leen las iteraciones del propio registro.
/// </summary>
public sealed class HashPbkdf2 : IHashContrasena
{
    private const string Algoritmo = "pbkdf2-sha256";
    private const int IteracionesPredeterminadas = 120_000;
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    public string Hashear(string contrasena)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(BytesSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            contrasena,
            sal,
            IteracionesPredeterminadas,
            HashAlgorithmName.SHA256,
            BytesHash);

        return string.Join(
            '$',
            Algoritmo,
            IteracionesPredeterminadas.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToBase64String(sal),
            Convert.ToBase64String(hash));
    }

    public bool Verificar(string contrasena, string hashAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(hashAlmacenado))
            return false;

        string[] partes = hashAlmacenado.Split('$');

        if (partes.Length != 4 || partes[0] != Algoritmo)
            return false;

        if (!int.TryParse(partes[1], out int iteraciones) || iteraciones <= 0)
            return false;

        try
        {
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);

            if (sal.Length == 0 || esperado.Length == 0)
                return false;

            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(
                contrasena,
                sal,
                iteraciones,
                HashAlgorithmName.SHA256,
                esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
