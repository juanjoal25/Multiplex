using Cadena.Application.Abstractions;
using Cadena.Domain.Repositories;
using Cadena.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Cadena.Domain.Events;

namespace Cadena.Application.UseCases.Contratos;

public sealed record RegistrarContratoCommand(Guid IdSucursal, string Tercero, DateTime VigenciaInicio, DateTime VigenciaFin, string Condiciones) : IRequest<Guid>;
public sealed record CancelarContratoCommand(Guid IdContrato, string Motivo) : IRequest<Unit>;

public sealed class RegistrarContratoHandler(ISucursalRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<RegistrarContratoCommand, Guid>
{
    public async Task<Guid> Handle(RegistrarContratoCommand cmd, CancellationToken ct)
    {
        var s = await repo.GetByIdAsync(SucursalId.Of(cmd.IdSucursal), ct)
            ?? throw new PreconditionFailedException("Sucursal no existe");
        var inicio = DateTime.SpecifyKind(cmd.VigenciaInicio, DateTimeKind.Utc);
        var fin = DateTime.SpecifyKind(cmd.VigenciaFin, DateTimeKind.Utc);
        var contrato = s.RegistrarContrato(cmd.Tercero, Vigencia.Of(inicio, fin), cmd.Condiciones);
        await repo.UpdateAsync(s, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in s.DomainEvents.OfType<DomainEvents.ContratoCorporativoRegistrado>())
            await publisher.PublishAsync(new Messaging.Contracts.Cadena.ContratoCorporativoRegistrado(
                e.IdContrato.Value, e.IdSucursal.Value, e.Tercero, e.Vigencia.FechaInicio, e.Vigencia.FechaFin, e.OccurredOn), ct);
        s.ClearEvents();
        return contrato.Id.Value;
    }
}

public sealed class CancelarContratoHandler(ISucursalRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<CancelarContratoCommand, Unit>
{
    public async Task<Unit> Handle(CancelarContratoCommand cmd, CancellationToken ct)
    {
        var idContrato = ContratoId.Of(cmd.IdContrato);
        var s = await repo.GetByContratoIdAsync(idContrato, ct)
            ?? throw new PreconditionFailedException("Contrato no existe");
        s.CancelarContrato(idContrato, cmd.Motivo);
        await repo.UpdateAsync(s, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in s.DomainEvents.OfType<DomainEvents.ContratoCorporativoCancelado>())
            await publisher.PublishAsync(new Messaging.Contracts.Cadena.ContratoCorporativoCancelado(
                e.IdContrato.Value, e.IdSucursal.Value, e.Motivo, e.OccurredOn), ct);
        s.ClearEvents();
        return Unit.Value;
    }
}
