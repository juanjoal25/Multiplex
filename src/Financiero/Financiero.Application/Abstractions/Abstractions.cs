using Financiero.Domain.ValueObjects;

namespace Financiero.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull;
}

public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct = default); }

public interface IPasarelaClient
{
    Task<(EstadoPago estado, ReferenciaExterna? referencia, string? motivo)> ProcesarPagoAsync(
        OrdenDepurada orden, MetodoPago metodo, CancellationToken ct);
}
