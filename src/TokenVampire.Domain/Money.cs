namespace TokenVampire.Domain;

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Of(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("ISO 4217 code required", nameof(currency));
        return new(amount, currency.ToUpperInvariant());
    }
    public Money Add(Money o) => o.Currency == Currency ? new(Amount + o.Amount, Currency)
        : throw new InvalidOperationException("Currency mismatch");
}
