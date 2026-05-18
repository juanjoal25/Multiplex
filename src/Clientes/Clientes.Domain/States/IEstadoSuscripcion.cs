using Clientes.Domain.ValueObjects;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.States;

public enum EstadoSuscripcionTipo { Activa, Expirada, Cancelada }

public interface IEstadoSuscripcion
{
    EstadoSuscripcionTipo Tipo { get; }
    bool AplicaDescuento { get; }
    PorcentajeDescuento DescuentoBase();
    IEstadoSuscripcion Cancelar();
    IEstadoSuscripcion Expirar();
    IEstadoSuscripcion Reactivar();
}

public sealed class SuscripcionActiva : IEstadoSuscripcion
{
    public EstadoSuscripcionTipo Tipo => EstadoSuscripcionTipo.Activa;
    public bool AplicaDescuento => true;
    public PorcentajeDescuento DescuentoBase() => PorcentajeDescuento.Zero;
    public IEstadoSuscripcion Cancelar() => new SuscripcionCancelada();
    public IEstadoSuscripcion Expirar() => new SuscripcionExpirada();
    public IEstadoSuscripcion Reactivar() => throw new PreconditionFailedException("Suscripción ya está activa");
}

public sealed class SuscripcionExpirada : IEstadoSuscripcion
{
    public EstadoSuscripcionTipo Tipo => EstadoSuscripcionTipo.Expirada;
    public bool AplicaDescuento => false;
    public PorcentajeDescuento DescuentoBase() => PorcentajeDescuento.Zero;
    public IEstadoSuscripcion Cancelar() => new SuscripcionCancelada();
    public IEstadoSuscripcion Expirar() => throw new PreconditionFailedException("Suscripción ya está expirada");
    public IEstadoSuscripcion Reactivar() => new SuscripcionActiva();
}

public sealed class SuscripcionCancelada : IEstadoSuscripcion
{
    public EstadoSuscripcionTipo Tipo => EstadoSuscripcionTipo.Cancelada;
    public bool AplicaDescuento => false;
    public PorcentajeDescuento DescuentoBase() => PorcentajeDescuento.Zero;
    public IEstadoSuscripcion Cancelar() => throw new PreconditionFailedException("Suscripción ya cancelada");
    public IEstadoSuscripcion Expirar() => throw new PreconditionFailedException("Suscripción cancelada no puede expirar");
    public IEstadoSuscripcion Reactivar() => throw new PreconditionFailedException("Suscripción CANCELADA no puede reactivarse - requiere nuevo registro");
}

public static class EstadoSuscripcionFactory
{
    public static IEstadoSuscripcion FromTipo(EstadoSuscripcionTipo tipo) => tipo switch
    {
        EstadoSuscripcionTipo.Activa => new SuscripcionActiva(),
        EstadoSuscripcionTipo.Expirada => new SuscripcionExpirada(),
        EstadoSuscripcionTipo.Cancelada => new SuscripcionCancelada(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
