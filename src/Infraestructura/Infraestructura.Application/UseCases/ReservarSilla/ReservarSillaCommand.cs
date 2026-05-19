using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Infraestructura.Domain.Events;

namespace Infraestructura.Application.UseCases.ReservarSilla;

public sealed record ReservarSillaCommand(Guid IdSilla, Guid IdFuncion, Guid IdOrden, DateTime Expiracion) : IRequest<Unit>;

public sealed class ReservarSillaHandler(ISalaRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<ReservarSillaCommand, Unit>
{
    public async Task<Unit> Handle(ReservarSillaCommand cmd, CancellationToken ct)
    {
        var sillaId = SillaId.Of(cmd.IdSilla);
        var sala = await repo.GetBySillaIdAsync(sillaId, ct)
            ?? throw new PreconditionFailedException("Sala/Silla no existen");

        var expVO = ReservaExpiracion.Of(cmd.Expiracion, DateTime.UtcNow);

        try
        {
            sala.ReservarSilla(sillaId, cmd.IdFuncion, cmd.IdOrden, expVO);
        }
        catch (ConflictException)
        {
            // El evento ReservaRechazada ya quedó registrado por el agregado
            foreach (var rej in sala.DomainEvents.OfType<DomainEvents.ReservaRechazada>())
                await publisher.PublishAsync(new Messaging.Contracts.Infraestructura.ReservaRechazada(
                    rej.IdSilla.Value, rej.IdFuncion, rej.IdOrden, rej.Motivo, rej.OccurredOn), ct);
            sala.ClearEvents();
            throw;
        }

        await repo.UpdateAsync(sala, ct);

        foreach (var e in sala.DomainEvents.OfType<DomainEvents.SillaReservada>())
            await publisher.PublishAsync(new Messaging.Contracts.Infraestructura.SillaReservada(
                e.IdSilla.Value, e.IdFuncion, e.IdOrden, e.Expiracion, e.OccurredOn), ct);

        await uow.SaveChangesAsync(ct);
        sala.ClearEvents();
        return Unit.Value;
    }
}
