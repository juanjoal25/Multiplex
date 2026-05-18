using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Infraestructura.Domain.Events;

namespace Infraestructura.Application.UseCases.Mantenimiento;

public sealed record EnviarMantenimientoCommand(Guid IdSala) : IRequest<Unit>;
public sealed record ReactivarSalaCommand(Guid IdSala) : IRequest<Unit>;

public sealed class EnviarMantenimientoHandler(ISalaRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<EnviarMantenimientoCommand, Unit>
{
    public async Task<Unit> Handle(EnviarMantenimientoCommand cmd, CancellationToken ct)
    {
        var sala = await repo.GetByIdAsync(SalaId.Of(cmd.IdSala), ct)
            ?? throw new PreconditionFailedException("Sala no existe");
        sala.EnviarMantenimiento();
        await repo.UpdateAsync(sala, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in sala.DomainEvents.OfType<DomainEvents.SalaEnMantenimiento>())
            await publisher.PublishAsync(new Messaging.Contracts.Infraestructura.SalaEnMantenimiento(e.IdSala.Value, e.OccurredOn), ct);
        sala.ClearEvents();
        return Unit.Value;
    }
}

public sealed class ReactivarSalaHandler(ISalaRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<ReactivarSalaCommand, Unit>
{
    public async Task<Unit> Handle(ReactivarSalaCommand cmd, CancellationToken ct)
    {
        var sala = await repo.GetByIdAsync(SalaId.Of(cmd.IdSala), ct)
            ?? throw new PreconditionFailedException("Sala no existe");
        sala.Reactivar();
        await repo.UpdateAsync(sala, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in sala.DomainEvents.OfType<DomainEvents.SalaReactivada>())
            await publisher.PublishAsync(new Messaging.Contracts.Infraestructura.SalaReactivada(e.IdSala.Value, e.OccurredOn), ct);
        sala.ClearEvents();
        return Unit.Value;
    }
}
