using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/financiero")]
[Authorize(Policy = "RequireAdmin")]
public sealed class FinancieroController(
    IFinancieroClient financiero,
    ILogger<FinancieroController> log) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int dias = 30, CancellationToken ct = default)
    {
        ViewData["AdminActive"] = "financiero";
        var inicio = DateTime.UtcNow.AddDays(-Math.Max(1, dias));
        var fin = DateTime.UtcNow.AddDays(1);

        IReadOnlyCollection<RegistroHistorialDto> registros = Array.Empty<RegistroHistorialDto>();
        try { registros = await financiero.ConsultarHistorialAsync(inicio, fin, ct); }
        catch (Exception ex) { log.LogWarning(ex, "Financiero microservice unreachable."); }

        return View(new FinancieroVm(registros, dias));
    }
}

public sealed record FinancieroVm(IReadOnlyCollection<RegistroHistorialDto> Registros, int Dias);
