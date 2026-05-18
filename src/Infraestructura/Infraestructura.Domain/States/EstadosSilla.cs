using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.States;

public enum EstadoSillaTipo { Disponible, Reservada, Ocupada }

public interface IEstadoSilla
{
    EstadoSillaTipo Tipo { get; }
    IEstadoSilla Reservar();
    IEstadoSilla Ocupar();
    IEstadoSilla Liberar();
}

public sealed class SillaDisponible : IEstadoSilla
{
    public EstadoSillaTipo Tipo => EstadoSillaTipo.Disponible;
    public IEstadoSilla Reservar() => new SillaReservadaEstado();
    public IEstadoSilla Ocupar() => throw new PreconditionFailedException("Silla DISPONIBLE no puede ocuparse directamente; debe reservarse antes");
    public IEstadoSilla Liberar() => throw new PreconditionFailedException("Silla DISPONIBLE no puede liberarse");
}

public sealed class SillaReservadaEstado : IEstadoSilla
{
    public EstadoSillaTipo Tipo => EstadoSillaTipo.Reservada;
    public IEstadoSilla Reservar() => throw new Shared.Kernel.Exceptions.ConflictException("Silla ya está RESERVADA");
    public IEstadoSilla Ocupar() => new SillaOcupadaEstado();
    public IEstadoSilla Liberar() => new SillaDisponible();
}

public sealed class SillaOcupadaEstado : IEstadoSilla
{
    public EstadoSillaTipo Tipo => EstadoSillaTipo.Ocupada;
    public IEstadoSilla Reservar() => throw new Shared.Kernel.Exceptions.ConflictException("Silla ya está OCUPADA");
    public IEstadoSilla Ocupar() => throw new PreconditionFailedException("Silla ya está OCUPADA");
    public IEstadoSilla Liberar() => new SillaDisponible();
}

public static class EstadoSillaFactory
{
    public static IEstadoSilla FromTipo(EstadoSillaTipo t) => t switch
    {
        EstadoSillaTipo.Disponible => new SillaDisponible(),
        EstadoSillaTipo.Reservada => new SillaReservadaEstado(),
        EstadoSillaTipo.Ocupada => new SillaOcupadaEstado(),
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };
}
