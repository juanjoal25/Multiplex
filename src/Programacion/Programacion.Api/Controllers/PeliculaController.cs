using MediatR;
using Microsoft.AspNetCore.Mvc;
using Programacion.Application.UseCases.RegistrarPelicula;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;

namespace Programacion.Api.Controllers;

[ApiController]
[Route("v1/programacion/peliculas")]
public sealed class PeliculaController(IMediator mediator) : ControllerBase
{
    public sealed record CrearPeliculaRequest(string Titulo, Clasificacion Clasificacion, string Genero, int DuracionMinutos, TipoFormato Formato);

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear([FromBody] CrearPeliculaRequest r, CancellationToken ct)
    {
        var id = await mediator.Send(new RegistrarPeliculaCommand(r.Titulo, r.Clasificacion, r.Genero, r.DuracionMinutos, r.Formato), ct);
        return CreatedAtAction(nameof(Crear), new { id }, new { id });
    }
}
