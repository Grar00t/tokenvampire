namespace TokenVampire.Domain;

public sealed class CaseLedger
{
    private readonly Dictionary<string, Charge> _items = new(StringComparer.Ordinal);

    public int Count => _items.Count;

    public bool Add(Charge charge)
    {
        ArgumentNullException.ThrowIfNull(charge);
        return _items.TryAdd(charge.Id, charge);
    }

    public Money Consumption(string currency) => Sum(currency, c => c.IsConsumption);

    public Money Funded(string currency) => Sum(currency, c => c.Kind == ChargeKind.CreditTopUp);

    private Money Sum(string currency, Func<Charge, bool> pick)
    {
        var total = Money.Of(0m, currency);
        foreach (var c in _items.Values)
        {
            if (pick(c) && string.Equals(c.Amount.Currency, total.Currency, StringComparison.Ordinal))
            {
                total = total.Add(c.Amount);
            }
        }
        return total;
    }
}
