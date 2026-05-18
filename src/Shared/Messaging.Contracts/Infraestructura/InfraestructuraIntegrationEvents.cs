using Messaging.Contracts.Clientes;

namespace Messaging.Contracts.Infraestructura;

public sealed record SillaReservada(
    Guid IdSilla,
    Guid IdFuncion,
    Guid IdOrden,
    DateTime Expiracion,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record ReservaRechazada(
    Guid IdSilla,
    Guid IdFuncion,
    Guid IdOrden,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record SillaOcupada(
    Guid IdSilla,
    Guid IdFuncion,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record SillaLiberada(
    Guid IdSilla,
    Guid IdFuncion,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record ReservaExpirada(
    Guid IdSilla,
    Guid IdOrden,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record SalaEnMantenimiento(
    Guid IdSala,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record SalaReactivada(
    Guid IdSala,
    DateTime Timestamp
) : IntegrationEvent;
