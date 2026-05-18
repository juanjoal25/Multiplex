using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public sealed class PorcentajeDescuento : ValueObject
{
    public decimal Valor { get; }

    private PorcentajeDescuento(decimal valor) => Valor = valor;

    public static PorcentajeDescuento Of(decimal valor)
    {
        if (valor < 0m || valor > 1m)
            throw new InvariantViolationException($"PorcentajeDescuento debe estar entre 0.0 y 1.0 ({valor})");
        return new PorcentajeDescuento(decimal.Round(valor, 4));
    }

    public static PorcentajeDescuento Zero => new(0m);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Valor; }
    public override string ToString() => $"{Valor:P}";
}
