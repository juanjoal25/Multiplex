using Financiero.Application.UseCases.ConsultarHistorial;
using Financiero.Application.UseCases.RegistrarTransaccion;
using Financiero.Application.UseCases.RevertirTransaccion;
using Financiero.Domain.Repositories;
using Financiero.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Financiero.Api.Controllers;

[ApiController]
[Route("v1/financiera")]
public sealed class TransaccionController(IMediator mediator, ITransaccionRepository repo) : ControllerBase
{
    public sealed record ConceptoDto(string Descripcion, decimal Valor);
    public sealed record RegistrarRequest(
        Guid IdOrden,
        IReadOnlyCollection<ConceptoDto> Conceptos,
        IReadOnlyCollection<decimal> Descuentos,
        decimal ValorTotal,
        string Moneda,
        MetodoPago MetodoPago);

    [HttpPost("transacciones")]
    public async Task<ActionResult<Guid>> Registrar([FromBody] RegistrarRequest r, CancellationToken ct)
    {
        var id = await mediator.Send(new RegistrarTransaccionCommand(
            r.IdOrden,
            r.Conceptos.Select(c => new ConceptoInput(c.Descripcion, c.Valor)).ToList(),
            r.Descuentos, r.ValorTotal, r.Moneda, r.MetodoPago), ct);
        return Created($"/v1/financiera/transacciones/{id}", new { id });
    }

    [HttpGet("transacciones/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var t = await repo.GetByIdAsync(TransaccionId.Of(id), ct);
        if (t is null) return NotFound();
        return Ok(new
        {
            Id = t.Id.Value,
            IdOrden = t.Orden.IdOrden,
            ValorTotal = t.Orden.ValorTotal.Amount,
            Moneda = t.Orden.ValorTotal.Currency,
            Estado = t.EstadoPago.ToString(),
            Referencia = t.Referencia?.Valor
        });
    }

    public sealed record RevertirRequest(string Motivo);
    [HttpPost("transacciones/{id:guid}/revertir")]
    public async Task<IActionResult> Revertir(Guid id, [FromBody] RevertirRequest r, CancellationToken ct)
    { await mediator.Send(new RevertirTransaccionCommand(id, r.Motivo), ct); return NoContent(); }

    [HttpGet("historial")]
    public async Task<ActionResult<IReadOnlyCollection<RegistroHistorialDto>>> Historial(
        [FromQuery] DateTime inicio, [FromQuery] DateTime fin, CancellationToken ct)
        => Ok(await mediator.Send(new ConsultarHistorialQuery(inicio, fin), ct));
}
