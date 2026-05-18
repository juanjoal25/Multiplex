using Infraestructura.Domain.Repositories;
using Infraestructura.Domain.Services;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Application.UseCases.ConsultarDisponibilidad;

public sealed record SillaDto(Guid IdSilla, string Fila, int Columna, string Tipo, string Estado);
public sealed record ConsultarDisponibilidadQuery(Guid IdSala) : IRequest<DisponibilidadResult>;
public sealed record DisponibilidadResult(Guid IdSala, int Aforo, int Disponibles, int Ocupacion, IReadOnlyCollection<SillaDto> Sillas);

public sealed class ConsultarDisponibilidadHandler(ISalaRepository repo, SvcGestionAforo aforo)
    : IRequestHandler<ConsultarDisponibilidadQuery, DisponibilidadResult>
{
    public async Task<DisponibilidadResult> Handle(ConsultarDisponibilidadQuery q, CancellationToken ct)
    {
        var sala = await repo.GetByIdAsync(SalaId.Of(q.IdSala), ct)
            ?? throw new PreconditionFailedException("Sala no existe");
        var sillas = sala.Sillas
            .Select(s => new SillaDto(s.Id.Value, s.Posicion.Fila, s.Posicion.Columna, s.Tipo.ToString(), s.Estado.Tipo.ToString()))
            .ToList();
        return new(q.IdSala, sala.Aforo.Valor, aforo.CalcularDisponibles(sala), aforo.CalcularOcupacion(sala), sillas);
    }
}
