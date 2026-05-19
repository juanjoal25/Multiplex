using Programacion.Domain.Events;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.Aggregates.AlquilerAgg;

public enum PropositoAlquiler { ProyeccionPrivada, EventoCorporativo, Cumpleanos, Otro }
public enum EstadoAlquiler { Solicitado, Confirmado, Cancelado, Finalizado }

public sealed class Alquiler : AggregateRoot<AlquilerId>
{
    public SalaRef SalaRef { get; private set; }
    public RangoHorario Rango { get; private set; }
    public string Solicitante { get; private set; }
    public PropositoAlquiler Proposito { get; private set; }
    public EstadoAlquiler Estado { get; private set; }
    public int CapacidadReservada { get; private set; }

    private Alquiler() { SalaRef = null!; Rango = null!; Solicitante = null!; }
    private Alquiler(AlquilerId id, SalaRef sala, RangoHorario rango, string solicitante, PropositoAlquiler prop, EstadoAlquiler est, int cap) : base(id)
    {
        SalaRef = sala; Rango = rango; Solicitante = solicitante; Proposito = prop; Estado = est; CapacidadReservada = cap;
    }

    public static Alquiler Registrar(SalaRef sala, RangoHorario rango, string solicitante, PropositoAlquiler prop, int cap)
    {
        if (string.IsNullOrWhiteSpace(solicitante))
            throw new InvariantViolationException("Solicitante requerido");
        if (cap < 0) throw new InvariantViolationException("CapacidadReservada no negativa");
        var id = AlquilerId.New();
        var a = new Alquiler(id, sala, rango, solicitante.Trim(), prop, EstadoAlquiler.Solicitado, cap);
        a.Raise(new AlquilerRegistrado(id, sala, rango, solicitante.Trim()));
        return a;
    }

    public void Confirmar()
    {
        if (Estado != EstadoAlquiler.Solicitado)
            throw new PreconditionFailedException("Solo un alquiler SOLICITADO puede confirmarse");
        Estado = EstadoAlquiler.Confirmado;
    }

    public void Cancelar()
    {
        if (Estado is EstadoAlquiler.Cancelado or EstadoAlquiler.Finalizado)
            throw new PreconditionFailedException($"Alquiler en estado {Estado} no puede cancelarse");
        Estado = EstadoAlquiler.Cancelado;
    }
}
