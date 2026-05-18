using Clientes.Domain.States;
using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.Aggregates.EspectadorAgg;

public sealed class Suscripcion : Entity<Guid>
{
    public IEstadoSuscripcion Estado { get; private set; }
    public INivel Nivel { get; private set; }
    public Vigencia? Vigencia { get; private set; }

    private Suscripcion(Guid id, IEstadoSuscripcion estado, INivel nivel, Vigencia? vigencia) : base(id)
    {
        Estado = estado;
        Nivel = nivel;
        Vigencia = vigencia;
    }

    public static Suscripcion Inicial() =>
        new(Guid.NewGuid(), new SuscripcionActiva(), new NivelNormal(), null);

    public static Suscripcion Restore(Guid id, EstadoSuscripcionTipo estado, TipoNivel nivel, Vigencia? vigencia) =>
        new(id, EstadoSuscripcionFactory.FromTipo(estado), NivelFactory.FromTipo(nivel), vigencia);

    public PorcentajeDescuento PorcentajeDescuento() =>
        Estado.AplicaDescuento ? Nivel.CalcularDescuento() : ValueObjects.PorcentajeDescuento.Zero;

    public void Activar(Vigencia vigencia)
    {
        if (Estado.Tipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Solo una suscripción ACTIVA puede actualizarse con vigencia");
        Vigencia = vigencia;
    }

    public (TipoNivel anterior, TipoNivel nuevo) Ascender()
    {
        if (Estado.Tipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Suscripción debe estar ACTIVA para ascender");
        if (!Nivel.PuedeAscender())
            throw new PreconditionFailedException($"Nivel {Nivel.Tipo} no puede ascender");
        var anterior = Nivel.Tipo;
        Nivel = Nivel.Ascender();
        return (anterior, Nivel.Tipo);
    }

    public (TipoNivel anterior, TipoNivel nuevo) Descender()
    {
        if (!Nivel.PuedeDescender())
            throw new PreconditionFailedException($"Nivel {Nivel.Tipo} no puede descender");
        var anterior = Nivel.Tipo;
        Nivel = Nivel.Descender();
        return (anterior, Nivel.Tipo);
    }

    public TipoNivel Expirar(DateTime ahora)
    {
        if (Estado.Tipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Solo una suscripción ACTIVA puede expirar");
        if (Vigencia is null || !Vigencia.HaExpirado(ahora))
            throw new PreconditionFailedException("Vigencia no ha vencido");
        var anterior = Nivel.Tipo;
        Estado = Estado.Expirar();
        return anterior;
    }

    public void Cancelar()
    {
        Estado = Estado.Cancelar();
    }

    public void Reactivar(Vigencia nuevaVigencia)
    {
        Estado = Estado.Reactivar();
        Vigencia = nuevaVigencia;
    }
}
