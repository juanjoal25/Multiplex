using Shared.Kernel.ValueObjects;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Services;

public sealed class ParametrosPrecio
{
    public Money PrecioBaseGeneral { get; init; } = Money.Of(15000m);
    public Money PrecioBaseVip { get; init; } = Money.Of(25000m);
    public Money PrecioBaseEspecial { get; init; } = Money.Of(35000m);
}

public sealed class SvcCalculoPrecio
{
    public Money CalcularPrecioBoleta(TipoBoleta tipoSilla, Money precioExtraFormato, ParametrosPrecio param)
    {
        var baseSilla = tipoSilla switch
        {
            TipoBoleta.General => param.PrecioBaseGeneral,
            TipoBoleta.Vip => param.PrecioBaseVip,
            TipoBoleta.Especial => param.PrecioBaseEspecial,
            _ => throw new ArgumentOutOfRangeException(nameof(tipoSilla))
        };
        return baseSilla.Add(precioExtraFormato);
    }

    public Money CalcularPrecioCombo(DefCombo combo, int cantidad) => combo.PrecioEspecial.Multiply(cantidad);

    public Money AplicarDescuento(Money subtotal, Descuento descuento) =>
        subtotal.Multiply(1m - descuento.Porcentaje);
}

public sealed class SvcValidacionEventoCorporativo
{
    public delegate Task<bool> ExisteContratoVigenteFn(string tercero, CancellationToken ct);
    public delegate Task<bool> SalaTieneFuncionesEnRangoFn(Guid idSala, DateTime inicio, DateTime fin, CancellationToken ct);

    public async Task<bool> Validar(string tercero, Guid idSala, DateTime inicio, DateTime fin,
        ExisteContratoVigenteFn existeContrato,
        SalaTieneFuncionesEnRangoFn tieneFunciones,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tercero);
        if (idSala == Guid.Empty) throw new ArgumentException(nameof(idSala));
        if (fin <= inicio) throw new ArgumentException("Rango inválido");
        var contratoOk = await existeContrato(tercero, ct);
        if (!contratoOk) return false;
        var solapa = await tieneFunciones(idSala, inicio, fin, ct);
        return !solapa;
    }
}
