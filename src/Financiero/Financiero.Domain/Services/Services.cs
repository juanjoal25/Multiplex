using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.ValueObjects;
using Shared.Kernel.Exceptions;

namespace Financiero.Domain.Services;

public sealed class SvcProcesoPago
{
    public delegate Task<(EstadoPago estado, ReferenciaExterna? referencia, string? motivo)> ProcesarFn(
        OrdenDepurada orden, MetodoPago metodo, CancellationToken ct);

    public async Task ProcesarPago(Transaccion transaccion, ProcesarFn pasarela, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transaccion);
        var (estado, referencia, motivo) = await pasarela(transaccion.Orden, transaccion.MetodoPago, ct);
        switch (estado)
        {
            case EstadoPago.Aprobado:
                if (referencia is null) throw new InvariantViolationException("ReferenciaExterna requerida en aprobación");
                transaccion.AprobarPago(referencia);
                break;
            case EstadoPago.Rechazado:
                transaccion.RechazarPago(motivo ?? "Rechazo de pasarela");
                break;
            default:
                throw new InvariantViolationException($"Estado de pasarela no soportado: {estado}");
        }
    }
}

public sealed class SvcRegistroContable
{
    public RegistroContable? Obtener(Transaccion t) => t.Registro;
}
