using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;

namespace Programacion.Domain.Events;

public sealed record PeliculaRegistrada(PeliculaId IdPelicula, string Titulo, Clasificacion Clasificacion, string Genero, int DuracionMinutos) : DomainEvent;
public sealed record FuncionProgramadaEvento(FuncionId IdFuncion, PeliculaRef PeliculaRef, SalaRef SalaRef, RangoHorario Horario, TipoFormato Formato) : DomainEvent;
public sealed record FuncionIniciadaEvento(FuncionId IdFuncion, SalaRef SalaRef) : DomainEvent;
public sealed record FuncionFinalizadaEvento(FuncionId IdFuncion, SalaRef SalaRef) : DomainEvent;
public sealed record FuncionCanceladaEvento(FuncionId IdFuncion, SalaRef SalaRef, string Motivo) : DomainEvent;
public sealed record CarteleraActualizada(CarteleraId IdCartelera, IReadOnlyCollection<Guid> Funciones, PeriodoCartelera Periodo) : DomainEvent;
public sealed record AlquilerRegistrado(AlquilerId IdAlquiler, SalaRef SalaRef, RangoHorario Rango, string Tercero) : DomainEvent;
