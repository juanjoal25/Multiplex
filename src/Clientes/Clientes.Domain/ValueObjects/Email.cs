using System.Net.Mail;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvariantViolationException("Email no puede ser vacío");
        if (!MailAddress.TryCreate(value, out _))
            throw new InvariantViolationException($"Email '{value}' no cumple RFC 5321");
        return new Email(value.Trim().ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
