using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/funciones")]
[Authorize(Policy = "RequireAdmin")]
public sealed class FuncionesController(
    IProgramacionClient programacion,
    IInfraestructuraClient infra,
    ILogger<FuncionesController> log) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["AdminActive"] = "funciones";

        var details = new List<FuncionDetail>();
        try
        {
            var cartelera = await programacion.ConsultarCarteleraAsync(ct);
            foreach (var id in cartelera.Funciones.Take(30))
            {
                var d = await programacion.GetFuncionAsync(id, ct);
                if (d is not null) details.Add(d);
            }
        }
        catch (Exception ex) { log.LogWarning(ex, "Programacion microservice unreachable."); }

        return View(new FuncionesVm(details, programacion.ListarPeliculasStub(), infra.ListarSalasStub()));
    }

    [HttpPost("nueva")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(NuevaFuncionInput input, CancellationToken ct)
    {
        try
        {
            var id = await programacion.ProgramarFuncionAsync(new CrearFuncionRequest(
                input.IdPelicula, input.IdSala, input.Inicio, input.Fin, input.Formato), ct);
            TempData["Flash"] = $"Función programada ({id.ToString()[..8].ToUpper()}).";
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Programacion microservice rejected the funcion.");
            TempData["Flash"] = "No fue posible programar la función.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/cancelar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id, string motivo, CancellationToken ct)
    {
        try { await programacion.CancelarFuncionAsync(id, motivo ?? "Cancelación administrativa", ct); TempData["Flash"] = "Función cancelada."; }
        catch (Exception ex) { log.LogError(ex, "Cancel funcion failed."); TempData["Flash"] = "No fue posible cancelar."; }
        return RedirectToAction(nameof(Index));
    }
}

public sealed record FuncionesVm(
    IReadOnlyList<FuncionDetail> Funciones,
    IReadOnlyCollection<PeliculaSummary> Peliculas,
    IReadOnlyCollection<SalaSummary> Salas);

public sealed class NuevaFuncionInput
{
    public Guid IdPelicula { get; set; }
    public Guid IdSala { get; set; }
    public DateTime Inicio { get; set; } = DateTime.Today.AddDays(1).AddHours(19);
    public DateTime Fin { get; set; } = DateTime.Today.AddDays(1).AddHours(21);
    public TipoFormato Formato { get; set; } = TipoFormato.Formato2D;
}
