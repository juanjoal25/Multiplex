using Financiero.Domain.Events;
using Financiero.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;
using Shared.Kernel.ValueObjects;

namespace Financiero.Domain.Aggregates.TransaccionAgg;

public sealed class TransaccionReversion : Entity<Guid>
{
    public TransaccionId IdTransaccionOriginal { get; }
    public string Motivo { get; }
    public DateTime Timestamp { get; }

    private TransaccionReversion(Guid id, TransaccionId original, string motivo, DateTime ts) : base(id)
    { IdTransaccionOriginal = original; Motivo = motivo; Timestamp = ts; }

    public static TransaccionReversion Crear(TransaccionId original, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new PreconditionFailedException("Motivo requerido");
        return new(Guid.NewGuid(), original, motivo.Trim(), DateTime.UtcNow);
    }

    public static TransaccionReversion Restore(Guid id, TransaccionId original, string motivo, DateTime ts)
        => new(id, original, motivo, ts);
}

public sealed class Transaccion : AggregateRoot<TransaccionId>
{
    public OrdenDepurada Orden { get; }
    public MetodoPago MetodoPago { get; }
    public EstadoPago EstadoPago { get; private set; }
    public ReferenciaExterna? Referencia { get; private set; }
    public RegistroContable? Registro { get; private set; }
    public DateTime Timestamp { get; }
    public TransaccionReversion? Reversion { get; private set; }

    private Transaccion(TransaccionId id, OrdenDepurada orden, MetodoPago metodo, EstadoPago estado,
        ReferenciaExterna? ref_, RegistroContable? reg, DateTime ts, TransaccionReversion? rev) : base(id)
    {
        Orden = orden; MetodoPago = metodo; EstadoPago = estado;
        Referencia = ref_; Registro = reg; Timestamp = ts; Reversion = rev;
    }

    public static Transaccion Registrar(OrdenDepurada orden, MetodoPago metodo)
    {
        ArgumentNullException.ThrowIfNull(orden);
        var id = TransaccionId.New();
        var t = new Transaccion(id, orden, metodo, EstadoPago.Pendiente, null, null, DateTime.UtcNow, null);
        t.Raise(new TransaccionRegistrada(id, orden.IdOrden, orden.ValorTotal));
        return t;
    }

    public static Transaccion Restore(TransaccionId id, OrdenDepurada o, MetodoPago m, EstadoPago e,
        ReferenciaExterna? r, RegistroContable? reg, DateTime ts, TransaccionReversion? rev)
        => new(id, o, m, e, r, reg, ts, rev);

    public void AprobarPago(ReferenciaExterna referencia)
    {
        if (EstadoPago != EstadoPago.Pendiente)
            throw new PreconditionFailedException("Solo transacción PENDIENTE puede aprobarse");
        EstadoPago = EstadoPago.Aprobado;
        Referencia = referencia;
        Registro = RegistroContable.Of(Id, Orden.ValorTotal, EstadoPago.Aprobado, referencia);
        Raise(new PagoAprobado(Id, Orden.IdOrden, referencia.Valor));
    }

    public void RechazarPago(string motivo)
    {
        if (EstadoPago != EstadoPago.Pendiente)
            throw new PreconditionFailedException("Solo transacción PENDIENTE puede rechazarse");
        if (string.IsNullOrWhiteSpace(motivo)) throw new PreconditionFailedException("Motivo requerido");
        EstadoPago = EstadoPago.Rechazado;
        Raise(new PagoRechazado(Id, Orden.IdOrden, motivo));
    }

    public void Revertir(string motivo)
    {
        if (EstadoPago != EstadoPago.Aprobado)
            throw new PreconditionFailedException("Solo transacción APROBADA puede revertirse");
        if (Reversion is not null)
            throw new PreconditionFailedException("Transacción ya revertida");
        Reversion = TransaccionReversion.Crear(Id, motivo);
        Raise(new TransaccionRevertida(Id, TransaccionId.New(), motivo));
    }
}
