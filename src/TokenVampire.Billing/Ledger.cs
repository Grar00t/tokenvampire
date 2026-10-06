using TokenVampire.Domain;

namespace TokenVampire.Billing;

public sealed record LedgerSummary(
    IReadOnlyDictionary<string, decimal> TopUps,
    IReadOnlyDictionary<string, decimal> Consumption,
    IReadOnlyDictionary<string, decimal> Credits,
    IReadOnlyDictionary<string, decimal> Refunds,
    IReadOnlyList<string> DuplicateIds);

public static class Ledger
{
    public static LedgerSummary Summarize(IEnumerable<Charge> charges)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var dups = new List<string>();
        var unique = new List<Charge>();
        foreach (var c in charges)
        {
            if (seen.Add(c.Id)) unique.Add(c); else dups.Add(c.Id);
        }

        static Dictionary<string, decimal> Sum(IEnumerable<Charge> xs) =>
            xs.GroupBy(x => x.Amount.Currency)
              .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount.Amount));

        return new(
            Sum(unique.Where(c => c.Kind == ChargeKind.CreditTopUp)),
            Sum(unique.Where(c => c.Kind == ChargeKind.UsageConsumption)),
            Sum(unique.Where(c => c.Kind == ChargeKind.Credit)),
            Sum(unique.Where(c => c.Kind == ChargeKind.Refund)),
            dups);
    }
}
