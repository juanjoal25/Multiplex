using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Services;

namespace Multiplex.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/salas")]
[Authorize(Policy = "RequireAdmin")]
public sealed class SalasController(
    IInfraestructuraClient infra,
    ILogger<SalasController> log) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["AdminActive"] = "salas";
        return View(infra.ListarSalasStub());
    }

    [HttpPost("{id:guid}/mantenimiento")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mantenimiento(Guid id, CancellationToken ct)
    {
        try { await infra.EnviarMantenimientoAsync(id, ct); TempData["Flash"] = "Sala enviada a mantenimiento."; }
        catch (Exception ex) { log.LogError(ex, "Mantenimiento failed."); TempData["Flash"] = "Operación falló."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/reactivar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivar(Guid id, CancellationToken ct)
    {
        try { await infra.ReactivarAsync(id, ct); TempData["Flash"] = "Sala reactivada."; }
        catch (Exception ex) { log.LogError(ex, "Reactivar failed."); TempData["Flash"] = "Operación falló."; }
        return RedirectToAction(nameof(Index));
    }
}
