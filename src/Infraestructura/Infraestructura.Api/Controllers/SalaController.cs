using Infraestructura.Application.UseCases.ConsultarDisponibilidad;
using Infraestructura.Application.UseCases.LiberarSilla;
using Infraestructura.Application.UseCases.Mantenimiento;
using Infraestructura.Application.UseCases.ReservarSilla;
using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Infraestructura.Api.Controllers;

[ApiController]
[Route("v1/infraestructura/salas")]
public sealed class SalaController(IMediator mediator, ISalaRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var salas = await repo.GetAllAsync(ct);
        return Ok(salas.Select(s => new
        {
            Id = s.Id.Value,
            s.Nombre,
            Tipo = s.Tipo.ToString(),
            Estado = s.Estado.Tipo.ToString(),
            Aforo = s.Aforo.Valor,
            Disponibles = s.Sillas.Count(x => x.Estado.Tipo == Infraestructura.Domain.States.EstadoSillaTipo.Disponible)
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var s = await repo.GetByIdAsync(SalaId.Of(id), ct);
        if (s is null) return NotFound();
        return Ok(new { Id = s.Id.Value, s.Nombre, Tipo = s.Tipo.ToString(), Estado = s.Estado.Tipo.ToString(), Aforo = s.Aforo.Valor });
    }

    [HttpGet("{id:guid}/disponibilidad")]
    public async Task<ActionResult<DisponibilidadResult>> Disponibilidad(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new ConsultarDisponibilidadQuery(id), ct));

    [HttpPost("{id:guid}/mantenimiento")]
    public async Task<IActionResult> Mantenimiento(Guid id, CancellationToken ct)
    { await mediator.Send(new EnviarMantenimientoCommand(id), ct); return NoContent(); }

    [HttpPost("{id:guid}/reactivar")]
    public async Task<IActionResult> Reactivar(Guid id, CancellationToken ct)
    { await mediator.Send(new ReactivarSalaCommand(id), ct); return NoContent(); }
}

[ApiController]
[Route("v1/infraestructura/sillas")]
public sealed class SillaController(IMediator mediator, ISalaRepository repo) : ControllerBase
{
    public sealed record ReservarRequest(Guid IdFuncion, Guid IdOrden, DateTime Expiracion);
    public sealed record LiberarRequest(string Motivo);

    [HttpPut("{id:guid}/reservar")]
    public async Task<IActionResult> Reservar(Guid id, [FromBody] ReservarRequest r, CancellationToken ct)
    { await mediator.Send(new ReservarSillaCommand(id, r.IdFuncion, r.IdOrden, r.Expiracion), ct); return NoContent(); }

    [HttpPost("{id:guid}/liberar")]
    public async Task<IActionResult> Liberar(Guid id, [FromBody] LiberarRequest r, CancellationToken ct)
    { await mediator.Send(new LiberarSillaCommand(id, r.Motivo), ct); return NoContent(); }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var sala = await repo.GetBySillaIdAsync(SillaId.Of(id), ct);
        var silla = sala?.Sillas.FirstOrDefault(s => s.Id.Value == id);
        if (silla is null) return NotFound();
        return Ok(new { Id = silla.Id.Value, Tipo = silla.Tipo.ToString(), Estado = silla.Estado.Tipo.ToString() });
    }
}
