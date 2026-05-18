using Shared.Kernel;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Events;

public sealed record OrdenCreada(OrdenId IdOrden, EspectadorRef Espectador, IReadOnlyCollection<Guid> Sillas, Money Total, DateTime Expiracion) : DomainEvent;
public sealed record OrdenConfirmada(OrdenId IdOrden, Money ValorTotal) : DomainEvent;
public sealed record OrdenExpirada(OrdenId IdOrden, EspectadorRef Espectador, IReadOnlyCollection<Guid> Sillas) : DomainEvent;
public sealed record OrdenCancelada(OrdenId IdOrden, EspectadorRef Espectador, IReadOnlyCollection<Guid> Sillas, string Motivo) : DomainEvent;
public sealed record StockAgotado(ProductoId IdProducto, string Nombre) : DomainEvent;
public sealed record StockReducido(ProductoId IdProducto, int Cantidad, int StockResultante) : DomainEvent;
public sealed record ComboDesactivado(DefComboId IdDefCombo, string Nombre) : DomainEvent;
