using Clientes.Domain.States;
using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.Aggregates.EspectadorAgg;

public sealed class Suscripcion : Entity<Guid>
{
    private EstadoSuscripcionTipo _estadoTipo;
    private TipoNivel _nivelTipo;

    public IEstadoSuscripcion Estado => EstadoSuscripcionFactory.FromTipo(_estadoTipo);
    public INivel Nivel => NivelFactory.FromTipo(_nivelTipo);
    public Vigencia? Vigencia { get; private set; }

    private Suscripcion() { }

    private Suscripcion(Guid id, IEstadoSuscripcion estado, INivel nivel, Vigencia? vigencia) : base(id)
    {
        _estadoTipo = estado.Tipo;
        _nivelTipo = nivel.Tipo;
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
        if (_estadoTipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Solo una suscripción ACTIVA puede actualizarse con vigencia");
        Vigencia = vigencia;
    }

    public (TipoNivel anterior, TipoNivel nuevo) Ascender()
    {
        if (_estadoTipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Suscripción debe estar ACTIVA para ascender");
        if (!Nivel.PuedeAscender())
            throw new PreconditionFailedException($"Nivel {_nivelTipo} no puede ascender");
        var anterior = _nivelTipo;
        _nivelTipo = Nivel.Ascender().Tipo;
        return (anterior, _nivelTipo);
    }

    public (TipoNivel anterior, TipoNivel nuevo) Descender()
    {
        if (!Nivel.PuedeDescender())
            throw new PreconditionFailedException($"Nivel {_nivelTipo} no puede descender");
        var anterior = _nivelTipo;
        _nivelTipo = Nivel.Descender().Tipo;
        return (anterior, _nivelTipo);
    }

    public TipoNivel Expirar(DateTime ahora)
    {
        if (_estadoTipo != EstadoSuscripcionTipo.Activa)
            throw new PreconditionFailedException("Solo una suscripción ACTIVA puede expirar");
        if (Vigencia is null || !Vigencia.HaExpirado(ahora))
            throw new PreconditionFailedException("Vigencia no ha vencido");
        var anterior = _nivelTipo;
        _estadoTipo = Estado.Expirar().Tipo;
        return anterior;
    }

    public void Cancelar()
    {
        _estadoTipo = Estado.Cancelar().Tipo;
    }

    public void Reactivar(Vigencia nuevaVigencia)
    {
        _estadoTipo = Estado.Reactivar().Tipo;
        Vigencia = nuevaVigencia;
    }
}
