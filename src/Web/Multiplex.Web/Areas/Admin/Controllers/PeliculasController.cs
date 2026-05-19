using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/peliculas")]
[Authorize(Policy = "RequireAdmin")]
public sealed class PeliculasController(
    IProgramacionClient programacion,
    ILogger<PeliculasController> log) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["AdminActive"] = "peliculas";
        return View(programacion.ListarPeliculasStub());
    }

    [HttpPost("nueva")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(NuevaPeliculaInput input, CancellationToken ct)
    {
        try
        {
            var id = await programacion.RegistrarPeliculaAsync(new CrearPeliculaRequest(
                input.Titulo, input.Clasificacion, input.Genero, input.DuracionMinutos, input.Formato), ct);
            TempData["Flash"] = $"Película creada ({id.ToString()[..8].ToUpper()}).";
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Programacion microservice rejected the movie.");
            TempData["Flash"] = "No fue posible crear la película.";
        }
        return RedirectToAction(nameof(Index));
    }
}

public sealed class NuevaPeliculaInput
{
    public string Titulo { get; set; } = string.Empty;
    public Clasificacion Clasificacion { get; set; } = Clasificacion.PG13;
    public string Genero { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; } = 100;
    public TipoFormato Formato { get; set; } = TipoFormato.Formato2D;
}
