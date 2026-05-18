using MediatR;
using Shared.Kernel.Exceptions;
using Ventas.Application.Abstractions;
using Ventas.Domain.Repositories;
using Ventas.Domain.ValueObjects;
using DomainEvents = Ventas.Domain.Events;

namespace Ventas.Application.UseCases.CancelarOrden;

public sealed record CancelarOrdenCommand(Guid IdOrden, string Motivo) : IRequest<Unit>;

public sealed class CancelarOrdenHandler(IOrdenRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<CancelarOrdenCommand, Unit>
{
    public async Task<Unit> Handle(CancelarOrdenCommand cmd, CancellationToken ct)
    {
        var o = await repo.GetByIdAsync(OrdenId.Of(cmd.IdOrden), ct)
            ?? throw new PreconditionFailedException("Orden no existe");
        o.Cancelar(cmd.Motivo);
        await repo.UpdateAsync(o, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in o.DomainEvents.OfType<DomainEvents.OrdenCancelada>())
            await publisher.PublishAsync(new Messaging.Contracts.Ventas.OrdenCancelada(
                e.IdOrden.Value, e.Espectador.Value, e.Sillas, e.Motivo, e.OccurredOn), ct);
        o.ClearEvents();
        return Unit.Value;
    }
}
