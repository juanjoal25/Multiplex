using Programacion.Domain.Events;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.Aggregates.CarteleraAgg;

public interface IObserverCartelera
{
    void Actualizar(CarteleraActualizada evento);
}

public sealed class Cartelera : AggregateRoot<CarteleraId>
{
    private readonly HashSet<Guid> _funciones = new();
    private readonly List<IObserverCartelera> _observadores = new();

    public PeriodoCartelera Periodo { get; private set; }
    public IReadOnlyCollection<Guid> Funciones => _funciones;

    private Cartelera() { Periodo = null!; }
    private Cartelera(CarteleraId id, PeriodoCartelera periodo, IEnumerable<Guid>? funciones = null) : base(id)
    {
        Periodo = periodo;
        if (funciones is not null) foreach (var f in funciones) _funciones.Add(f);
    }

    public static Cartelera Crear(PeriodoCartelera periodo) => new(CarteleraId.New(), periodo);

    public static Cartelera Restore(CarteleraId id, PeriodoCartelera p, IEnumerable<Guid> funciones)
        => new(id, p, funciones);

    public void AttachObserver(IObserverCartelera o) => _observadores.Add(o);

    public void Agregar(FuncionId idFuncion)
    {
        if (!_funciones.Add(idFuncion.Value))
            throw new InvariantViolationException("Función ya está en la Cartelera");
        var evt = new CarteleraActualizada(Id, _funciones.ToList(), Periodo);
        Raise(evt);
        foreach (var o in _observadores) o.Actualizar(evt);
    }

    public void Retirar(FuncionId idFuncion)
    {
        if (!_funciones.Remove(idFuncion.Value))
            throw new InvariantViolationException("Función no está en la Cartelera");
        var evt = new CarteleraActualizada(Id, _funciones.ToList(), Periodo);
        Raise(evt);
        foreach (var o in _observadores) o.Actualizar(evt);
    }
}
