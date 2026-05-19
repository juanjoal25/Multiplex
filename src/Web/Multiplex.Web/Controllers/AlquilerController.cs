using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

[Route("alquiler")]
public sealed class AlquilerController(
    ICadenaClient cadena,
    IInfraestructuraClient infra,
    ILogger<AlquilerController> log) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["NavActive"] = "alquiler";
        return View(new AlquilerVm(infra.ListarSalasStub(), null));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireCliente")]
    public async Task<IActionResult> Solicitar(AlquilerInput input, CancellationToken ct)
    {
        ViewData["NavActive"] = "alquiler";
        if (!ModelState.IsValid)
            return View("Index", new AlquilerVm(infra.ListarSalasStub(), null));

        try
        {
            var idContrato = await cadena.RegistrarContratoAsync(new CrearContratoRequest(
                cadena.SucursalPrincipalId(),
                input.Tercero,
                input.FechaInicio,
                input.FechaFin,
                $"Alquiler de sala {input.IdSala}. Contacto: {input.ContactoEmail}. Notas: {input.Notas}"),
                ct);
            TempData["Flash"] = $"Solicitud registrada. Contrato {idContrato.ToString()[..8].ToUpper()} en revisión.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Cadena microservice rejected the contract.");
            ModelState.AddModelError(string.Empty, "No fue posible registrar la solicitud.");
            return View("Index", new AlquilerVm(infra.ListarSalasStub(), input));
        }
    }
}

public sealed record AlquilerVm(IReadOnlyCollection<SalaSummary> Salas, AlquilerInput? Form);

public sealed class AlquilerInput
{
    public string Tercero { get; set; } = string.Empty;
    public string ContactoEmail { get; set; } = string.Empty;
    public Guid IdSala { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(7);
    public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(7);
    public string Notas { get; set; } = string.Empty;
}
