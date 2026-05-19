using System.Security.Claims;

namespace Multiplex.Web.Auth;

/// Propaga el IdEspectador del usuario autenticado como header X-Espectador-Id
/// hacia los microservicios. Los servicios NO autentican; el BFF es la frontera.
public sealed class EspectadorIdHandler(IHttpContextAccessor http) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var idEsp = http.HttpContext?.User?.FindFirstValue("idEspectador");
        if (!string.IsNullOrWhiteSpace(idEsp))
        {
            request.Headers.Remove("X-Espectador-Id");
            request.Headers.Add("X-Espectador-Id", idEsp);
        }
        return base.SendAsync(request, ct);
    }
}

public static class AuthClaimsExtensions
{
    public static Guid? GetEspectadorId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("idEspectador");
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    public static string? GetNivel(this ClaimsPrincipal user)
        => user.FindFirstValue("nivel");

    public static string? GetDisplayName(this ClaimsPrincipal user)
        => user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email);
}
