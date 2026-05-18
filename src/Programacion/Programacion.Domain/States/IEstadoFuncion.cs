using Shared.Kernel.Exceptions;

namespace Programacion.Domain.States;

public enum EstadoFuncionTipo { Programada, EnCurso, Finalizada, Cancelada }

public interface IEstadoFuncion
{
    EstadoFuncionTipo Tipo { get; }
    bool EsModificable { get; }
    IEstadoFuncion Iniciar();
    IEstadoFuncion Finalizar();
    IEstadoFuncion Cancelar();
}

public sealed class FuncionProgramada : IEstadoFuncion
{
    public EstadoFuncionTipo Tipo => EstadoFuncionTipo.Programada;
    public bool EsModificable => true;
    public IEstadoFuncion Iniciar() => new FuncionEnCurso();
    public IEstadoFuncion Finalizar() => throw new PreconditionFailedException("FuncionProgramada no puede finalizar sin iniciarse");
    public IEstadoFuncion Cancelar() => new FuncionCancelada();
}

public sealed class FuncionEnCurso : IEstadoFuncion
{
    public EstadoFuncionTipo Tipo => EstadoFuncionTipo.EnCurso;
    public bool EsModificable => false;
    public IEstadoFuncion Iniciar() => throw new PreconditionFailedException("FuncionEnCurso ya está iniciada");
    public IEstadoFuncion Finalizar() => new FuncionFinalizada();
    public IEstadoFuncion Cancelar() => throw new PreconditionFailedException("FuncionEnCurso no puede cancelarse");
}

public sealed class FuncionFinalizada : IEstadoFuncion
{
    public EstadoFuncionTipo Tipo => EstadoFuncionTipo.Finalizada;
    public bool EsModificable => false;
    public IEstadoFuncion Iniciar() => throw new PreconditionFailedException("FuncionFinalizada inmutable");
    public IEstadoFuncion Finalizar() => throw new PreconditionFailedException("FuncionFinalizada inmutable");
    public IEstadoFuncion Cancelar() => throw new PreconditionFailedException("FuncionFinalizada inmutable");
}

public sealed class FuncionCancelada : IEstadoFuncion
{
    public EstadoFuncionTipo Tipo => EstadoFuncionTipo.Cancelada;
    public bool EsModificable => false;
    public IEstadoFuncion Iniciar() => throw new PreconditionFailedException("FuncionCancelada inmutable");
    public IEstadoFuncion Finalizar() => throw new PreconditionFailedException("FuncionCancelada inmutable");
    public IEstadoFuncion Cancelar() => throw new PreconditionFailedException("FuncionCancelada inmutable");
}

public static class EstadoFuncionFactory
{
    public static IEstadoFuncion FromTipo(EstadoFuncionTipo t) => t switch
    {
        EstadoFuncionTipo.Programada => new FuncionProgramada(),
        EstadoFuncionTipo.EnCurso => new FuncionEnCurso(),
        EstadoFuncionTipo.Finalizada => new FuncionFinalizada(),
        EstadoFuncionTipo.Cancelada => new FuncionCancelada(),
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };
}
