using Cadena.Application.UseCases.ActualizarConfiguracion;
using Cadena.Application.UseCases.ConsultarConfiguracion;
using Cadena.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cadena.Api.Controllers;

[ApiController]
[Route("v1/cadena")]
public sealed class SucursalController(IMediator mediator) : ControllerBase
{
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
