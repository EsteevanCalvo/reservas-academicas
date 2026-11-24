using Reservas.Infrastructure.Seguridad;

namespace Reservas.Tests.Pruebas;

public sealed class HashPbkdf2Tests
{
    private readonly HashPbkdf2 _hash = new();

    [Fact]
    public void Dos_hashes_de_la_misma_contrasena_son_distintos()
    {
        string primero = _hash.Hashear("ClaveSegura123!");
        string segundo = _hash.Hashear("ClaveSegura123!");

        Assert.NotEqual(primero, segundo);
    }

    [Fact]
    public void Verifica_la_contrasena_correcta()
    {
        string hash = _hash.Hashear("ClaveSegura123!");

        Assert.True(_hash.Verificar("ClaveSegura123!", hash));
    }

    [Fact]
    public void Rechaza_la_contrasena_incorrecta()
    {
        string hash = _hash.Hashear("ClaveSegura123!");

        Assert.False(_hash.Verificar("ClaveSegura124!", hash));
    }

    [Fact]
    public void Rechaza_un_hash_con_formato_invalido()
    {
        Assert.False(_hash.Verificar("ClaveSegura123!", "texto-que-no-es-un-hash"));
        Assert.False(_hash.Verificar("ClaveSegura123!", "pbkdf2-sha256$abc$def$ghi"));
        Assert.False(_hash.Verificar(string.Empty, string.Empty));
    }

    [Fact]
    public void El_hash_no_contiene_la_contrasena_en_claro()
    {
        const string contrasena = "ClaveSegura123!";
        string hash = _hash.Hashear(contrasena);

        Assert.DoesNotContain(contrasena, hash, StringComparison.Ordinal);
        Assert.StartsWith("pbkdf2-sha256$", hash, StringComparison.Ordinal);
    }
}
