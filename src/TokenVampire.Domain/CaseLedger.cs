namespace TokenVampire.Domain;

public sealed class CaseLedger
{
    private readonly Dictionary<string, Charge> _items = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _gaps = new(StringComparer.Ordinal);

    public int Count => _items.Count;

    public IReadOnlyCollection<Charge> Items => _items.Values;

    public IReadOnlyDictionary<string, string> Gaps => _gaps;

    public bool Add(Charge charge)
    {
        ArgumentNullException.ThrowIfNull(charge);
        return _items.TryAdd(charge.Id, charge);
    }

    public bool AddGap(string id, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return _gaps.TryAdd(id, description);
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
