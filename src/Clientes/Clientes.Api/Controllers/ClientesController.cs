using Clientes.Application.UseCases.AscenderNivel;
using Clientes.Application.UseCases.ConsultarDescuento;
using Clientes.Application.UseCases.DescenderNivel;
using Clientes.Application.UseCases.RegistrarEspectador;
using Clientes.Domain.ValueObjects;
using Clientes.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Api.Controllers;

[ApiController]
[Route("v1/clientes")]
public sealed class ClientesController(IMediator mediator, ClientesDbContext db) : ControllerBase
{
    public sealed record RegistroRequest(string Nombre, string Apellido, string Correo, TipoDocumento TipoDocumento, string NumeroDocumento);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var espectadores = await db.Espectadores.AsNoTracking().ToListAsync(ct);
        var lista = espectadores.Select(e => new
        {
            Id = e.Id.Value,
            Nombre = e.Nombre.Nombre + " " + e.Nombre.Apellido,
            Correo = e.Correo.Value,
            Nivel = e.Suscripcion.Nivel.Tipo.ToString()
        });
        return Ok(lista);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string? nombre, [FromQuery] string? documento, CancellationToken ct)
    {
        var espectadores = await db.Espectadores.AsNoTracking().ToListAsync(ct);
        var filtrados = espectadores.Where(e =>
            (string.IsNullOrWhiteSpace(nombre) ||
                (e.Nombre.Nombre + " " + e.Nombre.Apellido).Contains(nombre, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(documento) ||
                e.Documento.Numero.Contains(documento, StringComparison.OrdinalIgnoreCase)));
        return Ok(filtrados.Select(e => new
        {
            Id = e.Id.Value,
            Nombre = e.Nombre.Nombre + " " + e.Nombre.Apellido,
            Correo = e.Correo.Value,
            Documento = e.Documento.Tipo + " " + e.Documento.Numero,
            Nivel = e.Suscripcion.Nivel.Tipo.ToString()
        }));
    }

    [HttpPost("registro")]
    public async Task<ActionResult<RegistrarEspectadorResult>> Registrar([FromBody] RegistroRequest r, CancellationToken ct)
    {
        var result = await mediator.Send(new RegistrarEspectadorCommand(
            r.Nombre, r.Apellido, r.Correo, r.TipoDocumento, r.NumeroDocumento), ct);
        return CreatedAtAction(nameof(Descuento), new { id = result.IdEspectador }, result);
    }

    [HttpGet("{id:guid}/descuento")]
    public async Task<ActionResult<ConsultarDescuentoResult>> Descuento(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new ConsultarDescuentoQuery(id), ct));
}

[ApiController]
[Route("v1/clientes")]
public sealed class SuscripcionController(IMediator mediator) : ControllerBase
{
    public sealed record AscenderRequest(Guid IdOrdenPago);

    [HttpPost("{id:guid}/ascender")]
    public async Task<IActionResult> Ascender(Guid id, [FromBody] AscenderRequest r, CancellationToken ct)
    { await mediator.Send(new AscenderNivelCommand(id, r.IdOrdenPago), ct); return NoContent(); }

    [HttpPost("{id:guid}/descender")]
    public async Task<IActionResult> Descender(Guid id, CancellationToken ct)
    { await mediator.Send(new DescenderNivelCommand(id), ct); return NoContent(); }
}
