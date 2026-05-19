using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Auth;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

[Route("perfil")]
[Authorize(Policy = "RequireCliente")]
public sealed class PerfilController(
    IClientesClient clientes,
    IFinancieroClient financiero,
    ILogger<PerfilController> log) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["NavActive"] = "perfil";
        ConsultarDescuentoResult? descuento = null;
        if (User.GetEspectadorId() is { } id)
        {
            try { descuento = await clientes.ConsultarDescuentoAsync(id, ct); }
            catch (Exception ex) { log.LogWarning(ex, "Clientes microservice unreachable."); }
        }
        return View(new PerfilVm(
            User.GetDisplayName() ?? "Espectador",
            User.GetEspectadorId(),
            descuento?.Nivel ?? User.GetNivel() ?? "Normal",
            descuento?.Porcentaje ?? 0,
            descuento?.Estado ?? "Activa"));
    }

    [HttpGet("historial")]
    public async Task<IActionResult> Historial(CancellationToken ct)
    {
        ViewData["NavActive"] = "perfil";
        var inicio = DateTime.UtcNow.AddYears(-1);
        var fin = DateTime.UtcNow.AddDays(1);

        IReadOnlyCollection<RegistroHistorialDto> registros = Array.Empty<RegistroHistorialDto>();
        try
        {
            registros = await financiero.ConsultarHistorialAsync(inicio, fin, ct);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Financiero microservice unreachable for historial.");
        }
        return View(registros);
    }
}

public sealed record PerfilVm(
    string Nombre,
    Guid? IdEspectador,
    string Nivel,
    decimal Descuento,
    string Estado);
