using Clientes.Application.Abstractions;
using Clientes.Domain.Repositories;
using DomainEvents = Clientes.Domain.Events;
using Clientes.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;

namespace Clientes.Application.UseCases.DescenderNivel;

public sealed record DescenderNivelCommand(Guid IdEspectador) : IRequest<Unit>;

public sealed class DescenderNivelHandler(
    IEspectadorRepository repo,
    IEventPublisher publisher,
    IUnitOfWork uow) : IRequestHandler<DescenderNivelCommand, Unit>
{
    public async Task<Unit> Handle(DescenderNivelCommand cmd, CancellationToken ct)
    {
        var esp = await repo.GetByIdAsync(EspectadorId.Of(cmd.IdEspectador), ct)
            ?? throw new PreconditionFailedException("Espectador no existe");
        esp.Descender();
        await repo.UpdateAsync(esp, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var evt in esp.DomainEvents.OfType<DomainEvents.NivelDescendido>())
            await publisher.PublishAsync(new Messaging.Contracts.Clientes.NivelDescendido(
                evt.IdEspectador.Value, evt.NivelAnterior.ToString(), evt.NivelNuevo.ToString(), evt.OccurredOn), ct);
        esp.ClearEvents();
        return Unit.Value;
    }
}
