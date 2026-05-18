using Cadena.Application.UseCases.Contratos;
using Cadena.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cadena.Api.Controllers;

[ApiController]
[Route("v1/cadena/contratos")]
public sealed class ContratoController(IMediator mediator, ISucursalRepository repo) : ControllerBase
{
    public sealed record CrearRequest(Guid IdSucursal, string Tercero, DateTime VigenciaInicio, DateTime VigenciaFin, string Condiciones);

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear([FromBody] CrearRequest r, CancellationToken ct)
    {
        var id = await mediator.Send(new RegistrarContratoCommand(r.IdSucursal, r.Tercero, r.VigenciaInicio, r.VigenciaFin, r.Condiciones), ct);
        return Created($"/v1/cadena/contratos/{id}", new { id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancelar(Guid id, [FromQuery] string motivo, CancellationToken ct)
    { await mediator.Send(new CancelarContratoCommand(id, motivo), ct); return NoContent(); }

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? tercero, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tercero)) return BadRequest();
        var sucursales = await repo.GetTodasAsync(ct);
        var ahora = DateTime.UtcNow;
        var vigente = sucursales.SelectMany(s => s.Contratos)
            .Any(c => string.Equals(c.Tercero, tercero, StringComparison.OrdinalIgnoreCase)
                && c.Estado == Domain.ValueObjects.EstadoContrato.Vigente
                && c.Vigencia.EstaVigenteEn(ahora));
        return vigente ? Ok() : NotFound();
    }
}
