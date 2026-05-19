using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Models.Dtos;
using Multiplex.Web.Services;

namespace Multiplex.Web.Controllers;

[Route("peliculas")]
public sealed class PeliculasController(
    IProgramacionClient programacion,
    ILogger<PeliculasController> log) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalle(Guid id, CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";
        var p = programacion.GetPeliculaStub(id);
        if (p is null) return NotFound();

        var funciones = await TryFetchFuncionesAsync(ct);
        return View(new PeliculaDetalleVm(p, funciones));
    }

    [HttpGet("{id:guid}/funciones")]
    public async Task<IActionResult> Funciones(Guid id, CancellationToken ct)
    {
        ViewData["NavActive"] = "cartelera";
        var p = programacion.GetPeliculaStub(id);
        if (p is null) return NotFound();

        var funciones = await TryFetchFuncionesAsync(ct);
        return View(new PeliculaFuncionesVm(p, funciones));
    }

    private async Task<IReadOnlyList<FuncionDetail>> TryFetchFuncionesAsync(CancellationToken ct)
    {
        try
        {
            var cartelera = await programacion.ConsultarCarteleraAsync(ct);
            var details = new List<FuncionDetail>();
            foreach (var fid in cartelera.Funciones.Take(12))
            {
                var d = await programacion.GetFuncionAsync(fid, ct);
                if (d is not null) details.Add(d);
            }
            return details;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Programacion microservice unreachable while loading funciones.");
            return Array.Empty<FuncionDetail>();
        }
    }
}

public sealed record PeliculaDetalleVm(PeliculaSummary Pelicula, IReadOnlyList<FuncionDetail> Funciones);
public sealed record PeliculaFuncionesVm(PeliculaSummary Pelicula, IReadOnlyList<FuncionDetail> Funciones);
