using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.Aggregates.SalaAgg;

public sealed class Silla : Entity<SillaId>
{
    private EstadoSillaTipo _estadoTipo;

    public Posicion Posicion { get; private set; }
    public TipoSilla Tipo { get; private set; }
    public IEstadoSilla Estado => EstadoSillaFactory.FromTipo(_estadoTipo);
    public ReservaExpiracion? ReservaExpiracion { get; private set; }
    public Guid? IdFuncionReservada { get; private set; }
    public Guid? IdOrdenReservada { get; private set; }

    private Silla() { Posicion = null!; }

    private Silla(SillaId id, Posicion pos, TipoSilla tipo, IEstadoSilla estado, ReservaExpiracion? exp, Guid? funcion, Guid? orden) : base(id)
    {
        Posicion = pos; Tipo = tipo; _estadoTipo = estado.Tipo;
        ReservaExpiracion = exp; IdFuncionReservada = funcion; IdOrdenReservada = orden;
    }

    public static Silla Crear(Posicion pos, TipoSilla tipo)
        => new(SillaId.New(), pos, tipo, new SillaDisponible(), null, null, null);

    public static Silla Restore(SillaId id, Posicion pos, TipoSilla tipo, EstadoSillaTipo estado, ReservaExpiracion? exp, Guid? funcion, Guid? orden)
        => new(id, pos, tipo, EstadoSillaFactory.FromTipo(estado), exp, funcion, orden);

    internal void Reservar(Guid idFuncion, Guid idOrden, ReservaExpiracion expiracion)
    {
        if (idOrden == Guid.Empty) throw new PreconditionFailedException("idOrden requerido");
        _estadoTipo = Estado.Reservar().Tipo;
        ReservaExpiracion = expiracion;
        IdFuncionReservada = idFuncion;
        IdOrdenReservada = idOrden;
    }

    internal void Ocupar(Guid idFuncion)
    {
        if (IdFuncionReservada != idFuncion)
            throw new PreconditionFailedException("idFuncion no coincide con la reserva");
        _estadoTipo = Estado.Ocupar().Tipo;
    }

    internal void Liberar()
    {
        _estadoTipo = Estado.Liberar().Tipo;
        ReservaExpiracion = null;
        IdFuncionReservada = null;
        IdOrdenReservada = null;
    }

    public bool ReservaHaExpirado(DateTime ahora) =>
        _estadoTipo == EstadoSillaTipo.Reservada && ReservaExpiracion is not null && ReservaExpiracion.HaExpirado(ahora);
}
