using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/contratos")]
[Authorize(Policy = "RequireAdmin")]
public sealed class ContratosController(
    ICadenaClient cadena,
    ILogger<ContratosController> log) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["AdminActive"] = "contratos";
        return View(cadena.ListarContratosStub());
    }

    [HttpPost("nuevo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(NuevoContratoInput input, CancellationToken ct)
    {
        try
        {
            var id = await cadena.RegistrarContratoAsync(new CrearContratoRequest(
                cadena.SucursalPrincipalId(), input.Tercero, input.Inicio, input.Fin, input.Condiciones), ct);
            TempData["Flash"] = $"Contrato registrado ({id.ToString()[..8].ToUpper()}).";
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Cadena microservice rejected the contract.");
            TempData["Flash"] = "No fue posible registrar el contrato.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/cancelar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id, string motivo, CancellationToken ct)
    {
        try { await cadena.CancelarContratoAsync(id, motivo ?? "Cancelación administrativa", ct); TempData["Flash"] = "Contrato cancelado."; }
        catch (Exception ex) { log.LogError(ex, "Cancel contract failed."); TempData["Flash"] = "No fue posible cancelar."; }
        return RedirectToAction(nameof(Index));
    }
}

public sealed class NuevoContratoInput
{
    public string Tercero { get; set; } = string.Empty;
    public DateTime Inicio { get; set; } = DateTime.Today;
    public DateTime Fin { get; set; } = DateTime.Today.AddMonths(6);
    public string Condiciones { get; set; } = string.Empty;
}
