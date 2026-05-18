using Clientes.Domain.Repositories;
using Clientes.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;

namespace Clientes.Application.UseCases.ConsultarDescuento;

public sealed record ConsultarDescuentoQuery(Guid IdEspectador) : IRequest<ConsultarDescuentoResult>;
public sealed record ConsultarDescuentoResult(decimal Porcentaje, string Nivel, string Estado);

public sealed class ConsultarDescuentoHandler(IEspectadorRepository repo)
    : IRequestHandler<ConsultarDescuentoQuery, ConsultarDescuentoResult>
{
    public async Task<ConsultarDescuentoResult> Handle(ConsultarDescuentoQuery q, CancellationToken ct)
    {
        var esp = await repo.GetByIdAsync(EspectadorId.Of(q.IdEspectador), ct)
            ?? throw new PreconditionFailedException("Espectador no existe");
        var porc = esp.PorcentajeDescuento();
        return new(porc.Valor, esp.Suscripcion.Nivel.Tipo.ToString(), esp.Suscripcion.Estado.Tipo.ToString());
    }
}
