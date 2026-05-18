using Messaging.Contracts.Clientes;

namespace Messaging.Contracts.Ventas;

public sealed record ConceptoFacturableDto(string Descripcion, decimal Valor, string Moneda);
public sealed record DescuentoAplicadoDto(decimal Porcentaje, string NivelOrigen);

public sealed record OrdenCreada(
    Guid IdOrden,
    Guid IdEspectador,
    IReadOnlyCollection<Guid> SillasReservadas,
    IReadOnlyCollection<Guid> Funciones,
    decimal Total,
    DateTime Expiracion
) : IntegrationEvent;

public sealed record OrdenConfirmada(
    Guid IdOrden,
    IReadOnlyCollection<ConceptoFacturableDto> ConceptosFacturables,
    IReadOnlyCollection<DescuentoAplicadoDto> DescuentosAplicados,
    decimal ValorTotal,
    string Moneda
) : IntegrationEvent;

public sealed record OrdenExpirada(
    Guid IdOrden,
    Guid IdEspectador,
    IReadOnlyCollection<Guid> SillaRefs,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record OrdenCancelada(
    Guid IdOrden,
    Guid IdEspectador,
    IReadOnlyCollection<Guid> SillaRefs,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record StockAgotado(
    Guid IdProducto,
    string Nombre,
    DateTime Timestamp
) : IntegrationEvent;
