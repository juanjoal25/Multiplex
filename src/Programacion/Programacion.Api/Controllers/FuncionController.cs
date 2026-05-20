using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Programacion.Application.UseCases.CancelarFuncion;
using Programacion.Application.UseCases.ProgramarFuncion;
using Programacion.Domain.Repositories;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Programacion.Infrastructure.Persistence;

namespace Programacion.Api.Controllers;

[ApiController]
[Route("v1/programacion")]
public sealed class FuncionController(IMediator mediator, IFuncionRepository repo, ProgramacionDbContext db) : ControllerBase
{
    public sealed record CrearFuncionRequest(Guid IdPelicula, Guid IdSala, DateTime Inicio, DateTime Fin, TipoFormato Formato);

    [HttpGet("funciones")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var funciones = await db.Funciones.AsNoTracking().ToListAsync(ct);
        var lista = funciones.Select(f => new
        {
            Id = f.Id.Value,
            IdPelicula = f.PeliculaRef.IdPelicula,
            IdSala = f.SalaRef.IdSala,
            Inicio = f.Horario.Inicio,
            Fin = f.Horario.Fin,
            Formato = f.Formato.Tipo.ToString(),
            Estado = f.Estado.Tipo.ToString()
        });
        return Ok(lista);
    }

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
            IdPelicula = f.PeliculaRef.IdPelicula,
            IdSala = f.SalaRef.IdSala,
            TipoSala = f.SalaRef.Tipo?.ToString() ?? "",
            Formato = f.Formato.Tipo.ToString(),
            PrecioExtraFormato = f.Formato.PrecioExtra().Amount,
            Inicio = f.Horario.Inicio,
            Fin = f.Horario.Fin
        });
    }
}
