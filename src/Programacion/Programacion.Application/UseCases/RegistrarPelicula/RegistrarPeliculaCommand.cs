using MediatR;
using Programacion.Application.Abstractions;
using Programacion.Domain.Aggregates.PeliculaAgg;
using Programacion.Domain.Repositories;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using DomainEvents = Programacion.Domain.Events;

namespace Programacion.Application.UseCases.RegistrarPelicula;

public sealed record RegistrarPeliculaCommand(string Titulo, Clasificacion Clasificacion, string Genero, int DuracionMinutos, TipoFormato Formato) : IRequest<Guid>;

public sealed class RegistrarPeliculaHandler(IPeliculaRepository repo, IEventPublisher publisher, IUnitOfWork uow)
    : IRequestHandler<RegistrarPeliculaCommand, Guid>
{
    public async Task<Guid> Handle(RegistrarPeliculaCommand cmd, CancellationToken ct)
    {
        var p = Pelicula.Registrar(Titulo.Of(cmd.Titulo), cmd.Clasificacion, Genero.Of(cmd.Genero), Duracion.Of(cmd.DuracionMinutos), cmd.Formato);
        await repo.AddAsync(p, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in p.DomainEvents.OfType<DomainEvents.PeliculaRegistrada>())
            await publisher.PublishAsync(new Messaging.Contracts.Programacion.PeliculaRegistrada(
                e.IdPelicula.Value, e.Titulo, e.Clasificacion.ToString(), e.Genero, e.DuracionMinutos)
            { OccurredOn = e.OccurredOn }, ct);

        p.ClearEvents();
        return p.Id.Value;
    }
}
