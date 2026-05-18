using Messaging.Contracts.Clientes;

namespace Messaging.Contracts.Financiero;

public sealed record TransaccionRegistrada(
    Guid IdTransaccion,
    Guid IdOrden,
    decimal ValorTotal,
    string Moneda,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record PagoAprobado(
    Guid IdTransaccion,
    Guid IdOrden,
    string ReferenciaExterna,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record PagoRechazado(
    Guid IdTransaccion,
    Guid IdOrden,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record TransaccionRevertida(
    Guid IdTransaccionOriginal,
    Guid IdTransaccionReversion,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;
