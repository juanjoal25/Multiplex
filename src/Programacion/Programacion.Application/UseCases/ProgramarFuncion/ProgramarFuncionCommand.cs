using MediatR;
using Programacion.Application.Abstractions;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Repositories;
using Programacion.Domain.Services;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Shared.Kernel.Exceptions;
using DomainEvents = Programacion.Domain.Events;

namespace Programacion.Application.UseCases.ProgramarFuncion;

public sealed record ProgramarFuncionCommand(Guid IdPelicula, Guid IdSala, DateTime Inicio, DateTime Fin, TipoFormato Formato) : IRequest<Guid>;

public sealed class ProgramarFuncionHandler(
    IFuncionRepository funcRepo,
    IPeliculaRepository pelRepo,
    IInfraestructuraClient infra,
    SvcValidacionHorario validacion,
    IEventPublisher publisher,
    IUnitOfWork uow) : IRequestHandler<ProgramarFuncionCommand, Guid>
{
    public async Task<Guid> Handle(ProgramarFuncionCommand cmd, CancellationToken ct)
    {
        var pel = await pelRepo.GetByIdAsync(PeliculaId.Of(cmd.IdPelicula), ct)
            ?? throw new PreconditionFailedException("Pelicula no existe");
        if (!await infra.SalaExisteYDisponibleAsync(cmd.IdSala, ct))
            throw new PreconditionFailedException("Sala no disponible");

        var tipoStr = await infra.TipoSalaAsync(cmd.IdSala, ct);
        var tipoSala = Enum.TryParse<TipoSala>(tipoStr, true, out var ts) ? ts : (TipoSala?)null;

        var horario = RangoHorario.Of(cmd.Inicio, cmd.Fin);
        var existentes = await funcRepo.GetByVigentesEnSalaAsync(cmd.IdSala, ct);
        var sala = SalaRef.Of(cmd.IdSala, tipoSala);
        if (!validacion.ValidarDisponibilidadSala(sala, horario, existentes))
            throw new ConflictException("Sala con horario solapado");

        var formato = FormatoFactory.FromTipo(cmd.Formato);
        var func = Funcion.Programar(PeliculaRef.Of(pel.Id), sala, horario, formato);
        await funcRepo.AddAsync(func, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var e in func.DomainEvents.OfType<DomainEvents.FuncionProgramadaEvento>())
            await publisher.PublishAsync(new Messaging.Contracts.Programacion.FuncionProgramada(
                e.IdFuncion.Value, e.PeliculaRef.IdPelicula, e.SalaRef.IdSala,
                e.Horario.Inicio, e.Horario.Fin, e.Formato.ToString())
            { OccurredOn = e.OccurredOn }, ct);

        func.ClearEvents();
        return func.Id.Value;
    }
}
