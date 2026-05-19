using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

public sealed class HomeController(
    IProgramacionClient programacion,
    ILogger<HomeController> log) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";

        // Always show the seed/curated movie list (admin can publish via Programacion microservice
        // once the GET /v1/programacion/peliculas endpoint exists). Cartelera funciones are pulled
        // live to drive the "funciones disponibles hoy" header.
        var peliculas = programacion.ListarPeliculasStub();
        int funcionesHoy = 0;
        try
        {
            var cartelera = await programacion.ConsultarCarteleraAsync(ct);
            funcionesHoy = cartelera.Funciones.Count;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Programacion microservice unavailable for cartelera count.");
        }

        return View(new CarteleraVm(peliculas, funcionesHoy));
    }

    public IActionResult Proximamente()
    {
        ViewData["NavActive"] = "proximamente";
        var todas = programacion.ListarPeliculasStub();
        return View("Index", new CarteleraVm(
            todas.Where(p => string.Equals(p.Estado, "Proximamente", StringComparison.OrdinalIgnoreCase)).ToList(),
            0));
    }

    public IActionResult Error() => View();
}

public sealed record CarteleraVm(IReadOnlyCollection<PeliculaSummary> Peliculas, int FuncionesHoy);
