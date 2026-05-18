using Financiero.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.ValueObjects;

namespace Financiero.Domain.Events;

public sealed record TransaccionRegistrada(TransaccionId IdTransaccion, Guid IdOrden, Money ValorTotal) : DomainEvent;
public sealed record PagoAprobado(TransaccionId IdTransaccion, Guid IdOrden, string ReferenciaExterna) : DomainEvent;
public sealed record PagoRechazado(TransaccionId IdTransaccion, Guid IdOrden, string Motivo) : DomainEvent;
public sealed record TransaccionRevertida(TransaccionId IdOriginal, TransaccionId IdReversion, string Motivo) : DomainEvent;
