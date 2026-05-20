using Cadena.Application.UseCases.ActualizarConfiguracion;
using Cadena.Application.UseCases.ConsultarConfiguracion;
using Cadena.Domain.Repositories;
using Cadena.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cadena.Api.Controllers;

[ApiController]
[Route("v1/cadena")]
public sealed class SucursalController(IMediator mediator, ISucursalRepository repo) : ControllerBase
{
    [HttpGet("sucursales")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await repo.GetTodasAsync(ct);
        return Ok(lista.Select(s => new
        {
            Id = s.Id.Value,
            Nombre = s.Nombre.Valor,
            ZonaHoraria = s.Configuracion.ZonaHoraria,
            Moneda = s.Configuracion.Moneda,
            Contratos = s.Contratos.Count
        }));
    }

    [HttpGet("configuracion/{idSucursal:guid}")]
    public async Task<ActionResult<ConfiguracionResult>> Get(Guid idSucursal, CancellationToken ct)
        => Ok(await mediator.Send(new ConsultarConfiguracionQuery(idSucursal), ct));

    public sealed record ActualizarParametroDto(string Clave, string Valor, TipoParametro Tipo);
    public sealed record ActualizarRequest(IReadOnlyCollection<ActualizarParametroDto> Parametros);

    [HttpPut("configuracion/{idSucursal:guid}")]
    public async Task<IActionResult> Actualizar(Guid idSucursal, [FromBody] ActualizarRequest r, CancellationToken ct)
    {
        await mediator.Send(new ActualizarConfiguracionCommand(
            idSucursal,
            r.Parametros.Select(p => new ParametroInput(p.Clave, p.Valor, p.Tipo)).ToList()), ct);
        return NoContent();
    }
}
