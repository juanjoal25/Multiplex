using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public sealed class NombreCompleto : ValueObject
{
    public string Nombre { get; }
    public string Apellido { get; }

    private NombreCompleto() { Nombre = null!; Apellido = null!; }
    private NombreCompleto(string nombre, string apellido) { Nombre = nombre; Apellido = apellido; }

    public static NombreCompleto Of(string nombre, string apellido)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 2)
            throw new InvariantViolationException("Nombre requiere mínimo 2 caracteres");
        if (string.IsNullOrWhiteSpace(apellido) || apellido.Trim().Length < 2)
            throw new InvariantViolationException("Apellido requiere mínimo 2 caracteres");
        return new NombreCompleto(nombre.Trim(), apellido.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nombre;
        yield return Apellido;
    }

    public override string ToString() => $"{Nombre} {Apellido}";
}
