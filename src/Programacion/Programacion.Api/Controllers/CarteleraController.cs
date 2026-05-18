using MediatR;
using Microsoft.AspNetCore.Mvc;
using Programacion.Application.UseCases.ConsultarCartelera;

namespace Programacion.Api.Controllers;

[ApiController]
[Route("v1/programacion/cartelera")]
public sealed class CarteleraController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ConsultarCarteleraResult>> Get(CancellationToken ct)
        => Ok(await mediator.Send(new ConsultarCarteleraQuery(DateTime.UtcNow), ct));
}
