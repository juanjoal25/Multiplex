using Shared.Kernel.Exceptions;

namespace Shared.Kernel.ValueObjects;

public sealed class Money : ValueObject
{
    public const string DefaultCurrency = "COP";

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0) throw new InvariantViolationException("Money.Amount no puede ser negativo");
        if (string.IsNullOrWhiteSpace(currency)) throw new InvariantViolationException("Money.Currency requerido");
        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = DefaultCurrency) => Of(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0) throw new InvariantViolationException("Factor no puede ser negativo");
        return Of(Amount * factor, Currency);
    }

    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    public static Money operator *(Money a, decimal f) => a.Multiply(f);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvariantViolationException($"Operaciones de Money requieren misma moneda ({Currency} vs {other.Currency})");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
