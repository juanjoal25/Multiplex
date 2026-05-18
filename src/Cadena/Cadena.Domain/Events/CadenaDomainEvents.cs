using Cadena.Domain.ValueObjects;
using Shared.Kernel;

namespace Cadena.Domain.Events;

public sealed record ConfiguracionActualizada(SucursalId IdSucursal, IReadOnlyCollection<ParametroGlobal> ParametrosModificados) : DomainEvent;
public sealed record ContratoCorporativoRegistrado(ContratoId IdContrato, SucursalId IdSucursal, string Tercero, Vigencia Vigencia) : DomainEvent;
public sealed record ContratoCorporativoVencido(ContratoId IdContrato, SucursalId IdSucursal) : DomainEvent;
public sealed record ContratoCorporativoCancelado(ContratoId IdContrato, SucursalId IdSucursal, string Motivo) : DomainEvent;
