using System.Text;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CampanhasAPI.Application.Security;

public class JwtGenerator(IOptions<JwtOptions> options) : IJwtGenerator
{
    private readonly JwtOptions _opt = options.Value;

    public (string Token, DateTimeOffset ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(_opt.ExpiracaoMinutos);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = usuario.Email,
                [JwtRegisteredClaimNames.Name] = usuario.NomeCompleto,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                // RF-1: a role vai literalmente como GestorONG ou Doador
                ["role"] = usuario.Role.ToString()
            }
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiraEm);
    }
}
