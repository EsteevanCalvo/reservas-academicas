using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Seguridad;

public sealed class EmisorTokens : IEmisorTokens
{
    private readonly OpcionesJwt _opciones;

    public EmisorTokens(IOptions<OpcionesJwt> opciones)
    {
        _opciones = opciones.Value;
    }

    public (string Token, DateTime Expira) Emitir(Usuario usuario)
    {
        DateTime ahora = DateTime.UtcNow;
        DateTime expira = ahora.AddHours(_opciones.HorasValidez);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave)),
            SecurityAlgorithms.HmacSha256);

        var descriptor = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        string token = new JwtSecurityTokenHandler().WriteToken(descriptor);
        return (token, expira);
    }
}
