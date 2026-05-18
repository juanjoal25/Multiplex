using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Shared.Kernel;

namespace Clientes.Domain.Events;

public sealed record EspectadorRegistrado(EspectadorId IdEspectador, TipoNivel Nivel) : DomainEvent;
public sealed record MembresiaActivada(EspectadorId IdEspectador, TipoNivel Nivel, DateTime FechaInicio, DateTime FechaFin) : DomainEvent;
public sealed record NivelAscendido(EspectadorId IdEspectador, TipoNivel NivelAnterior, TipoNivel NivelNuevo) : DomainEvent;
public sealed record NivelDescendido(EspectadorId IdEspectador, TipoNivel NivelAnterior, TipoNivel NivelNuevo) : DomainEvent;
public sealed record MembresiaExpirada(EspectadorId IdEspectador, TipoNivel NivelAnterior) : DomainEvent;
public sealed record MembresiaCancelada(EspectadorId IdEspectador, string Motivo) : DomainEvent;
