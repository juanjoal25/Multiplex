using Financiero.Domain.Repositories;
using MediatR;

namespace Financiero.Application.UseCases.ConsultarHistorial;

public sealed record RegistroHistorialDto(Guid IdTransaccion, Guid IdOrden, decimal ValorTotal, string Moneda, string EstadoPago, DateTime Fecha);
public sealed record ConsultarHistorialQuery(DateTime Inicio, DateTime Fin) : IRequest<IReadOnlyCollection<RegistroHistorialDto>>;

public sealed class ConsultarHistorialHandler(ITransaccionRepository repo)
    : IRequestHandler<ConsultarHistorialQuery, IReadOnlyCollection<RegistroHistorialDto>>
{
    public async Task<IReadOnlyCollection<RegistroHistorialDto>> Handle(ConsultarHistorialQuery q, CancellationToken ct)
    {
        var lista = await repo.GetHistorialAsync(q.Inicio, q.Fin, ct);
        return lista.Select(t => new RegistroHistorialDto(
            t.Id.Value, t.Orden.IdOrden, t.Orden.ValorTotal.Amount, t.Orden.ValorTotal.Currency,
            t.EstadoPago.ToString(), t.Timestamp)).ToList();
    }
}
