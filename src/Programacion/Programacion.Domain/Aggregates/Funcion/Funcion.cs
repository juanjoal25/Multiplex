using Programacion.Domain.Events;
using Programacion.Domain.States;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.Aggregates.FuncionAgg;

public sealed class Funcion : AggregateRoot<FuncionId>
{
    private TipoFormato _formatoTipo;
    private EstadoFuncionTipo _estadoTipo;

    public PeliculaRef PeliculaRef { get; private set; }
    public SalaRef SalaRef { get; private set; }
    public RangoHorario Horario { get; private set; }
    public IFormatoProyeccion Formato => FormatoFactory.FromTipo(_formatoTipo);
    public IEstadoFuncion Estado => EstadoFuncionFactory.FromTipo(_estadoTipo);

    private Funcion() { PeliculaRef = null!; SalaRef = null!; Horario = null!; }

    private Funcion(FuncionId id, PeliculaRef p, SalaRef s, RangoHorario h, IFormatoProyeccion f, IEstadoFuncion e)
        : base(id) { PeliculaRef = p; SalaRef = s; Horario = h; _formatoTipo = f.Tipo; _estadoTipo = e.Tipo; }

    public static Funcion Programar(PeliculaRef pelicula, SalaRef sala, RangoHorario horario, IFormatoProyeccion formato)
    {
        ArgumentNullException.ThrowIfNull(pelicula);
        ArgumentNullException.ThrowIfNull(sala);
        ArgumentNullException.ThrowIfNull(horario);
        ArgumentNullException.ThrowIfNull(formato);

        if (sala.Tipo is { } tipoSala && !formato.EsCompatibleConSala(tipoSala))
            throw new InvariantViolationException($"Formato {formato.Tipo} no es compatible con sala {tipoSala}");
        if (sala.Tipo == TipoSala.Imax && formato.Tipo is not (TipoFormato.FormatoIMAX or TipoFormato.Formato3D))
            throw new InvariantViolationException("Sala IMAX solo admite IMAX o 3D");

        var id = FuncionId.New();
        var f = new Funcion(id, pelicula, sala, horario, formato, new FuncionProgramada());
        f.Raise(new Events.FuncionProgramadaEvento(id, pelicula, sala, horario, formato.Tipo));
        return f;
    }

    public static Funcion Restore(FuncionId id, PeliculaRef p, SalaRef s, RangoHorario h, TipoFormato f, EstadoFuncionTipo e)
        => new(id, p, s, h, FormatoFactory.FromTipo(f), EstadoFuncionFactory.FromTipo(e));

    public void Iniciar(DateTime ahora)
    {
        if (ahora < Horario.Inicio)
            throw new PreconditionFailedException("No se puede iniciar antes de Horario.Inicio");
        _estadoTipo = Estado.Iniciar().Tipo;
        Raise(new Events.FuncionIniciadaEvento(Id, SalaRef));
    }

    public void Finalizar(DateTime ahora)
    {
        if (ahora < Horario.Fin)
            throw new PreconditionFailedException("No se puede finalizar antes de Horario.Fin");
        _estadoTipo = Estado.Finalizar().Tipo;
        Raise(new Events.FuncionFinalizadaEvento(Id, SalaRef));
    }

    public void Cancelar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new PreconditionFailedException("Motivo requerido");
        _estadoTipo = Estado.Cancelar().Tipo;
        Raise(new Events.FuncionCanceladaEvento(Id, SalaRef, motivo));
    }
}
