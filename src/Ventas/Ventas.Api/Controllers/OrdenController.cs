using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ventas.Application.UseCases.CancelarOrden;
using Ventas.Application.UseCases.ConfirmarOrden;
using Ventas.Application.UseCases.CrearOrden;

namespace Ventas.Api.Controllers;

[ApiController]
[Route("v1/ventas/orden")]
public sealed class OrdenController(IMediator mediator) : ControllerBase
{
    public sealed record BoletaDto(Guid IdFuncion, Guid IdSilla);
    public sealed record ConfiteriaDto(Guid IdProducto, int Cantidad);
    public sealed record CrearOrdenRequest(
        Guid IdEspectador,
        IReadOnlyCollection<BoletaDto> Boletas,
        IReadOnlyCollection<ConfiteriaDto> Confiterias,
        int MinutosExpiracion,
        bool EsEventoCorporativo,
        string? TerceroCorporativo);

    [HttpPost]
    public async Task<ActionResult<CrearOrdenResult>> Crear([FromBody] CrearOrdenRequest r, CancellationToken ct)
    {
        var result = await mediator.Send(new CrearOrdenCommand(
            r.IdEspectador,
            r.Boletas.Select(b => new BoletaInput(b.IdFuncion, b.IdSilla)).ToList(),
            r.Confiterias.Select(c => new ConfiteriaInput(c.IdProducto, c.Cantidad)).ToList(),
            r.MinutosExpiracion,
            r.EsEventoCorporativo,
            r.TerceroCorporativo), ct);
        return Created($"/v1/ventas/orden/{result.IdOrden}", result);
    }

    [HttpPost("{id:guid}/confirmar")]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    { await mediator.Send(new ConfirmarOrdenCommand(id), ct); return NoContent(); }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancelar(Guid id, [FromQuery] string motivo, CancellationToken ct)
    { await mediator.Send(new CancelarOrdenCommand(id, motivo), ct); return NoContent(); }
}
