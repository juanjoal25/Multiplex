namespace Messaging.Contracts.Clientes;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public sealed record EspectadorRegistrado(
    Guid IdEspectador,
    string Nivel,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record MembresiaActivada(
    Guid IdEspectador,
    string Nivel,
    DateTime FechaInicio,
    DateTime FechaFin
) : IntegrationEvent;

public sealed record NivelAscendido(
    Guid IdEspectador,
    string NivelAnterior,
    string NivelNuevo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record NivelDescendido(
    Guid IdEspectador,
    string NivelAnterior,
    string NivelNuevo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record MembresiaCancelada(
    Guid IdEspectador,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;
