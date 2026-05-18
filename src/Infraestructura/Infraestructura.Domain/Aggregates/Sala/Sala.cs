using Infraestructura.Domain.Events;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Domain.Aggregates.SalaAgg;

public sealed class Sala : AggregateRoot<SalaId>
{
    private readonly List<Silla> _sillas = new();

    public string Nombre { get; private set; }
    public TipoSala Tipo { get; private set; }
    public Aforo Aforo { get; private set; }
    public IEstadoSala Estado { get; private set; }
    public IReadOnlyCollection<Silla> Sillas => _sillas.AsReadOnly();

    private Sala(SalaId id, string nombre, TipoSala tipo, Aforo aforo, IEstadoSala estado, IEnumerable<Silla>? sillas = null) : base(id)
    {
        Nombre = nombre; Tipo = tipo; Aforo = aforo; Estado = estado;
        if (sillas is not null) _sillas.AddRange(sillas);
    }

    public static Sala Crear(string nombre, TipoSala tipo, Aforo aforo)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new InvariantViolationException("Nombre requerido");
        return new Sala(SalaId.New(), nombre.Trim(), tipo, aforo, new SalaDisponible());
    }

    public static Sala Restore(SalaId id, string nombre, TipoSala tipo, Aforo aforo, EstadoSalaTipo estado, IEnumerable<Silla> sillas)
        => new(id, nombre, tipo, aforo, EstadoSalaFactory.FromTipo(estado), sillas);

    public void AgregarSilla(Silla silla)
    {
        if (_sillas.Count >= Aforo.Valor)
            throw new InvariantViolationException("Sillas activas no pueden superar el Aforo");
        if (_sillas.Any(s => s.Posicion == silla.Posicion))
            throw new InvariantViolationException("Posición duplicada en la sala");
        if (Tipo == TipoSala.Vip && silla.Tipo is not (TipoSilla.Vip or TipoSilla.Acompanante))
            throw new InvariantViolationException("Sala VIP solo admite sillas VIP o ACOMPANANTE");
        _sillas.Add(silla);
    }

    public void ReservarSilla(SillaId sillaId, Guid idFuncion, Guid idOrden, ReservaExpiracion expiracion)
    {
        if (!Estado.PermiteReservaSillas)
            throw new PreconditionFailedException($"Sala en estado {Estado.Tipo} no permite reservas");
        var silla = _sillas.FirstOrDefault(s => s.Id == sillaId)
            ?? throw new PreconditionFailedException("Silla no existe en esta sala");

        try
        {
            silla.Reservar(idFuncion, idOrden, expiracion);
            Raise(new SillaReservada(sillaId, idFuncion, idOrden, expiracion.Valor));
        }
        catch (Shared.Kernel.Exceptions.ConflictException ex)
        {
            Raise(new ReservaRechazada(sillaId, idFuncion, idOrden, ex.Message));
            throw;
        }
    }

    public void OcuparSilla(SillaId sillaId, Guid idFuncion)
    {
        if (Estado.Tipo != EstadoSalaTipo.Ocupada)
            throw new PreconditionFailedException("Sala debe estar OCUPADA (función en curso) para ocupar sillas");
        var silla = _sillas.FirstOrDefault(s => s.Id == sillaId)
            ?? throw new PreconditionFailedException("Silla no existe");
        silla.Ocupar(idFuncion);
        Raise(new SillaOcupada(sillaId, idFuncion));
    }

    public void LiberarSilla(SillaId sillaId, string motivo)
    {
        var silla = _sillas.FirstOrDefault(s => s.Id == sillaId)
            ?? throw new PreconditionFailedException("Silla no existe");
        var idFuncion = silla.IdFuncionReservada ?? Guid.Empty;
        silla.Liberar();
        Raise(new SillaLiberada(sillaId, idFuncion, motivo));
    }

    public void EnviarMantenimiento()
    {
        if (_sillas.Any(s => s.Estado.Tipo is EstadoSillaTipo.Reservada or EstadoSillaTipo.Ocupada))
            throw new PreconditionFailedException("Sala con sillas RESERVADA u OCUPADA no puede ir a mantenimiento");
        Estado = Estado.EnviarMantenimiento();
        Raise(new SalaEnMantenimiento(Id));
    }

    public void Reactivar()
    {
        Estado = Estado.Reactivar();
        Raise(new SalaReactivada(Id));
    }

    public void IniciarFuncion() { Estado = Estado.IniciarFuncion(); }
    public void FinalizarFuncion()
    {
        Estado = Estado.FinalizarFuncion();
        foreach (var s in _sillas.Where(x => x.Estado.Tipo is EstadoSillaTipo.Reservada or EstadoSillaTipo.Ocupada))
            s.Liberar();
    }

    public IReadOnlyCollection<SillaId> LiberarExpiradas(DateTime ahora)
    {
        var liberadas = new List<SillaId>();
        foreach (var s in _sillas.Where(x => x.ReservaHaExpirado(ahora)).ToList())
        {
            var idFuncion = s.IdFuncionReservada ?? Guid.Empty;
            s.Liberar();
            liberadas.Add(s.Id);
            Raise(new SillaLiberada(s.Id, idFuncion, "EXPIRACION"));
        }
        return liberadas;
    }
}
