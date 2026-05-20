using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Programacion.Application.UseCases.RegistrarPelicula;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Programacion.Infrastructure.Persistence;

namespace Programacion.Api.Controllers;

[ApiController]
[Route("v1/programacion/peliculas")]
public sealed class PeliculaController(IMediator mediator, ProgramacionDbContext db) : ControllerBase
{
    public sealed record CrearPeliculaRequest(string Titulo, Clasificacion Clasificacion, string Genero, int DuracionMinutos, TipoFormato Formato);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await db.Peliculas.AsNoTracking().Select(p => new
        {
            Id = p.Id.Value,
            Titulo = p.Titulo.Valor,
            Clasificacion = p.Clasificacion.ToString(),
            Genero = p.Genero.Valor,
            Duracion = p.Duracion.Minutos
        }).ToListAsync(ct);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear([FromBody] CrearPeliculaRequest r, CancellationToken ct)
    {
        var id = await mediator.Send(new RegistrarPeliculaCommand(r.Titulo, r.Clasificacion, r.Genero, r.DuracionMinutos, r.Formato), ct);
        return CreatedAtAction(nameof(Crear), new { id }, new { id });
    }
}
