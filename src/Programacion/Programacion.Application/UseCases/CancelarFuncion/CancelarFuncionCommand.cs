using MediatR;
using Programacion.Application.Abstractions;
using Programacion.Domain.Repositories;
using Programacion.Domain.ValueObjects;
using Shared.Kernel.Exceptions;
using DomainEvents = Programacion.Domain.Events;

namespace Programacion.Application.UseCases.CancelarFuncion;

public sealed record CancelarFuncionCommand(Guid IdFuncion, string Motivo) : IRequest<Unit>;

public sealed class CancelarFuncionHandler(IFuncionRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<CancelarFuncionCommand, Unit>
{
    public async Task<Unit> Handle(CancelarFuncionCommand cmd, CancellationToken ct)
    {
        var f = await repo.GetByIdAsync(FuncionId.Of(cmd.IdFuncion), ct)
            ?? throw new PreconditionFailedException("Funcion no existe");
        f.Cancelar(cmd.Motivo);
        await repo.UpdateAsync(f, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in f.DomainEvents.OfType<DomainEvents.FuncionCanceladaEvento>())
            await publisher.PublishAsync(new Messaging.Contracts.Programacion.FuncionCancelada(
                e.IdFuncion.Value, e.SalaRef.IdSala, e.Motivo, e.OccurredOn), ct);

        f.ClearEvents();
        return Unit.Value;
    }
}
