using MediatR;
using Programacion.Domain.Repositories;

namespace Programacion.Application.UseCases.ConsultarCartelera;

public sealed record ConsultarCarteleraQuery(DateTime Ahora) : IRequest<ConsultarCarteleraResult>;
public sealed record ConsultarCarteleraResult(Guid? IdCartelera, IReadOnlyCollection<Guid> Funciones);

public sealed class ConsultarCarteleraHandler(ICarteleraRepository repo)
    : IRequestHandler<ConsultarCarteleraQuery, ConsultarCarteleraResult>
{
    public async Task<ConsultarCarteleraResult> Handle(ConsultarCarteleraQuery q, CancellationToken ct)
    {
        var c = await repo.GetVigenteAsync(q.Ahora, ct);
        return c is null
            ? new(null, Array.Empty<Guid>())
            : new(c.Id.Value, c.Funciones);
    }
}
