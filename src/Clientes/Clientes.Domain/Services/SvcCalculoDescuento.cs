using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;

namespace Clientes.Domain.Services;

public enum TipoCompra { Regular, Corporativo }

public sealed class SvcCalculoDescuento
{
    public PorcentajeDescuento Calcular(INivel nivel, TipoCompra tipoCompra)
    {
        ArgumentNullException.ThrowIfNull(nivel);
        if (tipoCompra == TipoCompra.Corporativo) return PorcentajeDescuento.Zero;
        return nivel.CalcularDescuento();
    }
}
