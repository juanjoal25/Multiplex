using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Multiplex.Web.Auth;

public sealed class JwtTokenService(IConfiguration cfg)
{
    private readonly string _key = cfg["Bff:Jwt:SigningKey"]!;
    private readonly string _issuer = cfg["Bff:Jwt:Issuer"]!;
    private readonly string _audience = cfg["Bff:Jwt:Audience"]!;
    private readonly int _hours = int.TryParse(cfg["Bff:Jwt:ExpiresHours"], out var h) ? h : 8;

    public (string Token, DateTime ExpiresUtc) Issue(User u, string? nivel = null)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, u.Email),
            new(ClaimTypes.Role, u.Role),
            new("name", u.Nombre ?? u.Email)
        };
        if (u.IdEspectador is { } idEsp)
            claims.Add(new Claim("idEspectador", idEsp.ToString()));
        if (!string.IsNullOrWhiteSpace(nivel))
            claims.Add(new Claim("nivel", nivel));

        var expires = DateTime.UtcNow.AddHours(_hours);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
