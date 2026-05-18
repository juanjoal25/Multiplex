using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.States;

public enum EstadoSalaTipo { Disponible, Ocupada, EnMantenimiento }

public interface IEstadoSala
{
    EstadoSalaTipo Tipo { get; }
    bool PermiteReservaSillas { get; }
    IEstadoSala EnviarMantenimiento();
    IEstadoSala Reactivar();
    IEstadoSala IniciarFuncion();
    IEstadoSala FinalizarFuncion();
}

public sealed class SalaDisponible : IEstadoSala
{
    public EstadoSalaTipo Tipo => EstadoSalaTipo.Disponible;
    public bool PermiteReservaSillas => true;
    public IEstadoSala EnviarMantenimiento() => new SalaEnMantenimientoEstado();
    public IEstadoSala Reactivar() => throw new PreconditionFailedException("Sala ya DISPONIBLE");
    public IEstadoSala IniciarFuncion() => new SalaOcupada();
    public IEstadoSala FinalizarFuncion() => throw new PreconditionFailedException("No hay función en curso");
}

public sealed class SalaOcupada : IEstadoSala
{
    public EstadoSalaTipo Tipo => EstadoSalaTipo.Ocupada;
    public bool PermiteReservaSillas => false;
    public IEstadoSala EnviarMantenimiento() => throw new PreconditionFailedException("Sala OCUPADA no puede ir a mantenimiento");
    public IEstadoSala Reactivar() => throw new PreconditionFailedException("Sala OCUPADA no necesita reactivarse");
    public IEstadoSala IniciarFuncion() => throw new PreconditionFailedException("Sala ya OCUPADA");
    public IEstadoSala FinalizarFuncion() => new SalaDisponible();
}

public sealed class SalaEnMantenimientoEstado : IEstadoSala
{
    public EstadoSalaTipo Tipo => EstadoSalaTipo.EnMantenimiento;
    public bool PermiteReservaSillas => false;
    public IEstadoSala EnviarMantenimiento() => throw new PreconditionFailedException("Ya EN_MANTENIMIENTO");
    public IEstadoSala Reactivar() => new SalaDisponible();
    public IEstadoSala IniciarFuncion() => throw new PreconditionFailedException("Sala EN_MANTENIMIENTO no admite funciones");
    public IEstadoSala FinalizarFuncion() => throw new PreconditionFailedException("Sala EN_MANTENIMIENTO no tiene función");
}

public static class EstadoSalaFactory
{
    public static IEstadoSala FromTipo(EstadoSalaTipo t) => t switch
    {
        EstadoSalaTipo.Disponible => new SalaDisponible(),
        EstadoSalaTipo.Ocupada => new SalaOcupada(),
        EstadoSalaTipo.EnMantenimiento => new SalaEnMantenimientoEstado(),
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };
}
