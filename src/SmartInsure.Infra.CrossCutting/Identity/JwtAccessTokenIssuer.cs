using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Infra.CrossCutting.Identity;

/// <summary>
/// Emite o acesso autenticado da plataforma como JWT assinado com a chave simétrica
/// compartilhada (ADR-015). O sub carrega a identidade externa — o mesmo claim que o
/// ICurrentUserAccessor lê na entrada.
/// </summary>
public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options) : IAccessTokenIssuer
{
    /// <summary>RN-005: acesso autenticado válido por 8 horas.</summary>
    internal static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(8);

    private readonly JwtOptions _options = options.Value;

    public AccessToken IssueFor(User user)
    {
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.Add(AccessTokenLifetime);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.ExternalIdentity),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ],
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
