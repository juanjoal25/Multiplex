using Clientes.Application.Abstractions;
using Clientes.Domain.Repositories;
using DomainEvents = Clientes.Domain.Events;
using Clientes.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;

namespace Clientes.Application.UseCases.AscenderNivel;

public sealed record AscenderNivelCommand(Guid IdEspectador, Guid IdOrdenPago) : IRequest<Unit>;

public sealed class AscenderNivelHandler(
    IEspectadorRepository repo,
    IFinancieroClient financiero,
    IEventPublisher publisher,
    IUnitOfWork uow) : IRequestHandler<AscenderNivelCommand, Unit>
{
    public async Task<Unit> Handle(AscenderNivelCommand cmd, CancellationToken ct)
    {
        if (!await financiero.PagoFueAprobadoAsync(cmd.IdOrdenPago, ct))
            throw new PreconditionFailedException("Pago del plan no aprobado en Financiero");

        var esp = await repo.GetByIdAsync(EspectadorId.Of(cmd.IdEspectador), ct)
            ?? throw new PreconditionFailedException("Espectador no existe");
        esp.Ascender();
        await repo.UpdateAsync(esp, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var evt in esp.DomainEvents.OfType<DomainEvents.NivelAscendido>())
            await publisher.PublishAsync(new Messaging.Contracts.Clientes.NivelAscendido(
                evt.IdEspectador.Value, evt.NivelAnterior.ToString(), evt.NivelNuevo.ToString(), evt.OccurredOn), ct);
        esp.ClearEvents();
        return Unit.Value;
    }
}
