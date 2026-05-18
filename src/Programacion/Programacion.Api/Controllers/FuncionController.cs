using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Programacion.Application.UseCases.CancelarFuncion;
using Programacion.Application.UseCases.ProgramarFuncion;
using Programacion.Domain.Repositories;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;

namespace Programacion.Api.Controllers;

[ApiController]
[Route("v1/programacion")]
public sealed class FuncionController(IMediator mediator, IFuncionRepository repo) : ControllerBase
{
    public sealed record CrearFuncionRequest(Guid IdPelicula, Guid IdSala, DateTime Inicio, DateTime Fin, TipoFormato Formato);

    [HttpPost("funciones")]
    public async Task<ActionResult<Guid>> Crear([FromBody] CrearFuncionRequest r, CancellationToken ct)
    {
        var id = await mediator.Send(new ProgramarFuncionCommand(r.IdPelicula, r.IdSala, r.Inicio, r.Fin, r.Formato), ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpDelete("funciones/{id:guid}")]
    public async Task<IActionResult> Cancelar(Guid id, [FromQuery] string motivo, CancellationToken ct)
    { await mediator.Send(new CancelarFuncionCommand(id, motivo), ct); return NoContent(); }

    [HttpGet("funcion/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var f = await repo.GetByIdAsync(FuncionId.Of(id), ct);
        if (f is null) return NotFound();
        return Ok(new
        {
            Id = f.Id.Value,
            IdSala = f.SalaRef.IdSala,
            TipoSala = f.SalaRef.Tipo?.ToString() ?? "",
            Formato = f.Formato.Tipo.ToString(),
            PrecioExtraFormato = f.Formato.PrecioExtra().Amount,
            Inicio = f.Horario.Inicio,
            Fin = f.Horario.Fin
        });
    }
}
