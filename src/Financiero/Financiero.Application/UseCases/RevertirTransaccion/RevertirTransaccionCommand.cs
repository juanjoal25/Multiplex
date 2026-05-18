using Financiero.Application.Abstractions;
using Financiero.Domain.Repositories;
using Financiero.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Financiero.Domain.Events;

namespace Financiero.Application.UseCases.RevertirTransaccion;

public sealed record RevertirTransaccionCommand(Guid IdTransaccion, string Motivo) : IRequest<Unit>;

public sealed class RevertirTransaccionHandler(ITransaccionRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<RevertirTransaccionCommand, Unit>
{
    public async Task<Unit> Handle(RevertirTransaccionCommand cmd, CancellationToken ct)
    {
        var t = await repo.GetByIdAsync(TransaccionId.Of(cmd.IdTransaccion), ct)
            ?? throw new PreconditionFailedException("Transaccion no existe");
        t.Revertir(cmd.Motivo);
        await repo.UpdateAsync(t, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in t.DomainEvents.OfType<DomainEvents.TransaccionRevertida>())
            await publisher.PublishAsync(new Messaging.Contracts.Financiero.TransaccionRevertida(
                e.IdOriginal.Value, e.IdReversion.Value, e.Motivo, e.OccurredOn), ct);
        t.ClearEvents();
        return Unit.Value;
    }
}
