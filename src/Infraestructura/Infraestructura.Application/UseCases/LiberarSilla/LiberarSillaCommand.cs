using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Infraestructura.Domain.Events;

namespace Infraestructura.Application.UseCases.LiberarSilla;

public sealed record LiberarSillaCommand(Guid IdSilla, string Motivo) : IRequest<Unit>;

public sealed class LiberarSillaHandler(ISalaRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<LiberarSillaCommand, Unit>
{
    public async Task<Unit> Handle(LiberarSillaCommand cmd, CancellationToken ct)
    {
        var id = SillaId.Of(cmd.IdSilla);
        var sala = await repo.GetBySillaIdAsync(id, ct)
            ?? throw new PreconditionFailedException("Silla no existe");

        sala.LiberarSilla(id, cmd.Motivo);
        await repo.UpdateAsync(sala, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in sala.DomainEvents.OfType<DomainEvents.SillaLiberada>())
            await publisher.PublishAsync(new Messaging.Contracts.Infraestructura.SillaLiberada(
                e.IdSilla.Value, e.IdFuncion, e.Motivo, e.OccurredOn), ct);

        sala.ClearEvents();
        return Unit.Value;
    }
}
