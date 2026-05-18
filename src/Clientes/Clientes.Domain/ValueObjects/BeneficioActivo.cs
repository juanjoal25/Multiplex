using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public enum TipoBeneficio { Descuento, Acceso, Regalo }

public sealed class BeneficioActivo : ValueObject
{
    public string Descripcion { get; }
    public TipoBeneficio Tipo { get; }
    public Vigencia Vigencia { get; }

    private BeneficioActivo(string descripcion, TipoBeneficio tipo, Vigencia vigencia)
    {
        Descripcion = descripcion; Tipo = tipo; Vigencia = vigencia;
    }

    public static BeneficioActivo Of(string descripcion, TipoBeneficio tipo, Vigencia vigencia)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new InvariantViolationException("BeneficioActivo.Descripcion requerida");
        return new BeneficioActivo(descripcion.Trim(), tipo, vigencia);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Descripcion;
        yield return Tipo;
        yield return Vigencia;
    }
}
