using Cadena.Application.Abstractions;
using Cadena.Domain.Repositories;
using Cadena.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Cadena.Domain.Events;

namespace Cadena.Application.UseCases.ActualizarConfiguracion;

public sealed record ParametroInput(string Clave, string Valor, TipoParametro Tipo);
public sealed record ActualizarConfiguracionCommand(Guid IdSucursal, IReadOnlyCollection<ParametroInput> Parametros) : IRequest<Unit>;

public sealed class ActualizarConfiguracionHandler(ISucursalRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<ActualizarConfiguracionCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarConfiguracionCommand cmd, CancellationToken ct)
    {
        var s = await repo.GetByIdAsync(SucursalId.Of(cmd.IdSucursal), ct)
            ?? throw new PreconditionFailedException("Sucursal no existe");
        var nuevos = cmd.Parametros.Select(p => ParametroGlobal.Of(p.Clave, p.Valor, p.Tipo)).ToList();
        s.ActualizarConfiguracion(nuevos);
        await repo.UpdateAsync(s, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in s.DomainEvents.OfType<DomainEvents.ConfiguracionActualizada>())
        {
            var mods = e.ParametrosModificados.Select(p =>
                new Messaging.Contracts.Cadena.ParametroModificadoDto(p.Clave, p.Valor, p.Tipo.ToString())).ToList();
            await publisher.PublishAsync(new Messaging.Contracts.Cadena.ConfiguracionActualizada(
                e.IdSucursal.Value, mods, e.OccurredOn), ct);
        }
        s.ClearEvents();
        return Unit.Value;
    }
}
