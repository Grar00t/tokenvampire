namespace TokenVampire.Domain;

public sealed record DamageEntry
{
    private DamageEntry(
        string id,
        DamageLedgerKind kind,
        Measured<decimal> amount,
        string currency,
        IReadOnlyList<string> evidenceIds)
    {
        Id = id;
        Kind = kind;
        Amount = amount;
        Currency = currency;
        EvidenceIds = evidenceIds;
    }

    public string Id { get; }
    public DamageLedgerKind Kind { get; }
    public Measured<decimal> Amount { get; }
    public string Currency { get; }
    public IReadOnlyList<string> EvidenceIds { get; }

    public static DamageEntry Create(
        string id,
        DamageLedgerKind kind,
        Measured<decimal> amount,
        string currency,
        IEnumerable<string> evidenceIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(evidenceIds);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "A canonical damage-ledger kind is required.");
        }

        var normalizedCurrency = Money.Of(0m, currency).Currency;
        var ids = evidenceIds
            .Select(x => string.IsNullOrWhiteSpace(x)
                ? throw new ArgumentException("Evidence ids cannot be blank.", nameof(evidenceIds))
                : x)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (amount.IsMeasured && ids.Length == 0)
        {
            throw new InvalidOperationException("A measured damage amount needs evidence.");
        }

        return new(id, kind, amount, normalizedCurrency, Array.AsReadOnly(ids));
    }
}

public sealed class DamageLedger
{
    private readonly Dictionary<string, DamageEntry> _entries = new(StringComparer.Ordinal);

    public IReadOnlyCollection<DamageEntry> Entries => _entries.Values;

    public bool Add(DamageEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Amount.IsMeasured && entry.EvidenceIds.Count == 0)
        {
            throw new InvalidOperationException("A measured damage amount needs evidence.");
        }

        return _entries.TryAdd(entry.Id, entry);
    }

    public Measured<Money> VerifiedTotal(string currency)
    {
        var normalizedCurrency = Money.Of(0m, currency).Currency;
        var entries = _entries.Values
            .Where(x => string.Equals(x.Currency, normalizedCurrency, StringComparison.Ordinal))
            .ToArray();

        if (entries.Length == 0 ||
            entries.Any(x => !x.Amount.IsMeasured || x.EvidenceIds.Count == 0))
        {
            return Measured<Money>.Unknown;
        }

        var total = entries.Sum(x => x.Amount.Value!.Value);
        return Measured<Money>.Of(Money.Of(total, normalizedCurrency));
    }
}
