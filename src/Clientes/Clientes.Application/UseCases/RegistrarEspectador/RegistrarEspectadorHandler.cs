using Clientes.Application.Abstractions;
using Clientes.Domain.Aggregates.EspectadorAgg;
using Clientes.Domain.Repositories;
using Clientes.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;
using DomainEvents = Clientes.Domain.Events;

namespace Clientes.Application.UseCases.RegistrarEspectador;

public sealed class RegistrarEspectadorHandler(
    IEspectadorRepository repo,
    IEventPublisher publisher,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarEspectadorCommand, RegistrarEspectadorResult>
{
    public async Task<RegistrarEspectadorResult> Handle(RegistrarEspectadorCommand cmd, CancellationToken ct)
    {
        var doc = Documento.Of(cmd.TipoDocumento, cmd.NumeroDocumento);
        if (await repo.ExistsByDocumentoAsync(doc, ct))
            throw new ConflictException("Ya existe un espectador con ese documento");

        var email = Email.Of(cmd.Correo);
        if (await repo.ExistsByEmailAsync(email, ct))
            throw new ConflictException("Ya existe un espectador con ese correo");

        var nombre = NombreCompleto.Of(cmd.Nombre, cmd.Apellido);
        var esp = Espectador.Registrar(nombre, email, doc);
        await repo.AddAsync(esp, ct);

        foreach (var evt in esp.DomainEvents.OfType<DomainEvents.EspectadorRegistrado>())
            await publisher.PublishAsync(new Messaging.Contracts.Clientes.EspectadorRegistrado(
                evt.IdEspectador.Value, evt.Nivel.ToString(), evt.OccurredOn), ct);

        await uow.SaveChangesAsync(ct);
        esp.ClearEvents();
        return new RegistrarEspectadorResult(esp.Id.Value);
    }
}
