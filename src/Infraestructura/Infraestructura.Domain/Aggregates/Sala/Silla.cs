using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.Aggregates.SalaAgg;

public sealed class Silla : Entity<SillaId>
{
    public Posicion Posicion { get; private set; }
    public TipoSilla Tipo { get; private set; }
    public IEstadoSilla Estado { get; private set; }
    public ReservaExpiracion? ReservaExpiracion { get; private set; }
    public Guid? IdFuncionReservada { get; private set; }
    public Guid? IdOrdenReservada { get; private set; }

    private Silla(SillaId id, Posicion pos, TipoSilla tipo, IEstadoSilla estado, ReservaExpiracion? exp, Guid? funcion, Guid? orden) : base(id)
    {
        Posicion = pos; Tipo = tipo; Estado = estado;
        ReservaExpiracion = exp; IdFuncionReservada = funcion; IdOrdenReservada = orden;
    }

    public static Silla Crear(Posicion pos, TipoSilla tipo)
        => new(SillaId.New(), pos, tipo, new SillaDisponible(), null, null, null);

    public static Silla Restore(SillaId id, Posicion pos, TipoSilla tipo, EstadoSillaTipo estado, ReservaExpiracion? exp, Guid? funcion, Guid? orden)
        => new(id, pos, tipo, EstadoSillaFactory.FromTipo(estado), exp, funcion, orden);

    internal void Reservar(Guid idFuncion, Guid idOrden, ReservaExpiracion expiracion)
    {
        if (idOrden == Guid.Empty) throw new PreconditionFailedException("idOrden requerido");
        Estado = Estado.Reservar();
        ReservaExpiracion = expiracion;
        IdFuncionReservada = idFuncion;
        IdOrdenReservada = idOrden;
    }

    internal void Ocupar(Guid idFuncion)
    {
        if (IdFuncionReservada != idFuncion)
            throw new PreconditionFailedException("idFuncion no coincide con la reserva");
        Estado = Estado.Ocupar();
    }

    internal void Liberar()
    {
        Estado = Estado.Liberar();
        ReservaExpiracion = null;
        IdFuncionReservada = null;
        IdOrdenReservada = null;
    }

    public bool ReservaHaExpirado(DateTime ahora) =>
        Estado.Tipo == EstadoSillaTipo.Reservada && ReservaExpiracion is not null && ReservaExpiracion.HaExpirado(ahora);
}
