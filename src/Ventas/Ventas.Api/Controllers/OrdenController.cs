using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Application.UseCases.CancelarOrden;
using Ventas.Application.UseCases.ConfirmarOrden;
using Ventas.Application.UseCases.CrearOrden;
using Ventas.Infrastructure.Persistence;

namespace Ventas.Api.Controllers;

[ApiController]
[Route("v1/ventas/orden")]
public sealed class OrdenController(IMediator mediator, VentasDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await db.Ordenes.AsNoTracking().Select(o => new
        {
            Id = o.Id.Value,
            IdEspectador = o.EspectadorRef.Value,
            Estado = o.Estado.ToString(),
            Total = o.Total.Amount,
            Moneda = o.Total.Currency,
            Expiracion = o.Expiracion.Valor
        }).ToListAsync(ct);
        return Ok(lista);
    }

    [HttpGet("/v1/ventas/productos")]
    public async Task<IActionResult> GetProductos(CancellationToken ct)
    {
        var lista = await db.Productos.AsNoTracking().Select(p => new
        {
            Id = p.Id.Value,
            p.Nombre,
            Precio = p.Precio.Amount,
            p.Stock
        }).ToListAsync(ct);
        return Ok(lista);
    }

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
