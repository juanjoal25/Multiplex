using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public enum TipoDocumento { CC, CE, PAS }

public sealed class Documento : ValueObject
{
    public TipoDocumento Tipo { get; }
    public string Numero { get; }

    private Documento() { Numero = null!; }
    private Documento(TipoDocumento tipo, string numero) { Tipo = tipo; Numero = numero; }

    public static Documento Of(TipoDocumento tipo, string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new InvariantViolationException("Documento.Numero no puede ser vacío");
        return new Documento(tipo, numero.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tipo;
        yield return Numero;
    }

    public override string ToString() => $"{Tipo}:{Numero}";
}
