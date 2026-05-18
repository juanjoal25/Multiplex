using Infraestructura.Domain.ValueObjects;
using Shared.Kernel;

namespace Infraestructura.Domain.Events;

public sealed record SillaReservada(SillaId IdSilla, Guid IdFuncion, Guid IdOrden, DateTime Expiracion) : DomainEvent;
public sealed record ReservaRechazada(SillaId IdSilla, Guid IdFuncion, Guid IdOrden, string Motivo) : DomainEvent;
public sealed record SillaOcupada(SillaId IdSilla, Guid IdFuncion) : DomainEvent;
public sealed record SillaLiberada(SillaId IdSilla, Guid IdFuncion, string Motivo) : DomainEvent;
public sealed record ReservaExpirada(SillaId IdSilla, Guid IdOrden) : DomainEvent;
public sealed record SalaEnMantenimiento(SalaId IdSala) : DomainEvent;
public sealed record SalaReactivada(SalaId IdSala) : DomainEvent;
