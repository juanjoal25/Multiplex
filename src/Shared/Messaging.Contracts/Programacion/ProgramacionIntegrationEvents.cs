using Messaging.Contracts.Clientes;

namespace Messaging.Contracts.Programacion;

public sealed record PeliculaRegistrada(
    Guid IdPelicula,
    string Titulo,
    string Clasificacion,
    string Genero,
    int DuracionMinutos
) : IntegrationEvent;

public sealed record FuncionProgramada(
    Guid IdFuncion,
    Guid IdPelicula,
    Guid IdSala,
    DateTime HorarioInicio,
    DateTime HorarioFin,
    string Formato
) : IntegrationEvent;

public sealed record FuncionIniciada(
    Guid IdFuncion,
    Guid IdSala,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record FuncionFinalizada(
    Guid IdFuncion,
    Guid IdSala,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record FuncionCancelada(
    Guid IdFuncion,
    Guid IdSala,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record CarteleraActualizada(
    Guid IdCartelera,
    IReadOnlyCollection<Guid> Funciones,
    DateTime PeriodoInicio,
    DateTime PeriodoFin
) : IntegrationEvent;
