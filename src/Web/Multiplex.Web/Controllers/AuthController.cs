using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Multiplex.Web.Auth;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

[Route("auth")]
public sealed class AuthController(
    BffIdentityDbContext db,
    PasswordHasher hasher,
    JwtTokenService jwt,
    IClientesClient clientes,
    ILogger<AuthController> log) : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) { ViewData["ReturnUrl"] = returnUrl; return View(vm); }

        var u = await db.Users.FirstOrDefaultAsync(x => x.Email == vm.Email, ct);
        if (u is null || !hasher.Verify(vm.Password, u.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        string? nivel = null;
        if (u.Role == "CLIENTE" && u.IdEspectador is { } idEsp)
        {
            try
            {
                var d = await clientes.ConsultarDescuentoAsync(idEsp, ct);
                nivel = d?.Nivel;
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Clientes microservice unreachable on login.");
            }
        }

        var (token, expires) = jwt.Issue(u, nivel);
        SetAuthCookie(token, expires);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return u.Role == "ADMIN"
            ? RedirectToAction("Index", "Peliculas", new { area = "Admin" })
            : RedirectToAction("Index", "Home");
    }

    [HttpGet("registro")]
    public IActionResult Registro() => View(new RegistroVm());

    [HttpPost("registro")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        if (vm.Password != vm.PasswordConfirm)
        {
            ModelState.AddModelError(nameof(vm.PasswordConfirm), "Las contraseñas no coinciden.");
            return View(vm);
        }
        if (await db.Users.AnyAsync(u => u.Email == vm.Email, ct))
        {
            ModelState.AddModelError(nameof(vm.Email), "Ya existe una cuenta con este correo.");
            return View(vm);
        }

        Guid idEspectador;
        try
        {
            var res = await clientes.RegistrarAsync(new RegistroRequest(
                vm.Nombre, vm.Apellido, vm.Email, vm.TipoDocumento, vm.NumeroDocumento), ct);
            idEspectador = res.IdEspectador;
        }
        catch (HttpRequestException ex)
        {
            log.LogError(ex, "Clientes microservice rejected the registration.");
            ModelState.AddModelError(string.Empty, "No fue posible registrar el espectador. Intenta de nuevo.");
            return View(vm);
        }

        var user = new User
        {
            Email = vm.Email,
            PasswordHash = hasher.Hash(vm.Password),
            Role = "CLIENTE",
            IdEspectador = idEspectador,
            Nombre = $"{vm.Nombre} {vm.Apellido}".Trim()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var (token, expires) = jwt.Issue(user, nivel: "Normal");
        SetAuthCookie(token, expires);

        TempData["Flash"] = "Cuenta creada. Bienvenido a FRAME.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        TempData["Flash"] = "Sesión cerrada.";
        return RedirectToAction("Index", "Home");
    }

    private void SetAuthCookie(string token, DateTime expiresUtc)
    {
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero),
            IsEssential = true
        });
    }
}

public sealed class LoginVm
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegistroVm
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.CC;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirm { get; set; } = string.Empty;
}
