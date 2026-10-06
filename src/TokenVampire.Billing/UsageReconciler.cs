using TokenVampire.Domain;

namespace TokenVampire.Billing;

public sealed record UsageLine(string Id, string Category, Measured<decimal> Tokens, Measured<decimal> Rate, Measured<decimal> Cost);

public enum ReconStatus { Matched, Mismatch, NotEstablished }

public sealed record ReconResult(ReconStatus Status, decimal? Difference, string Reason);

public static class UsageReconciler
{
    public static ReconResult Reconcile(IEnumerable<UsageLine> lines, Measured<decimal> invoiceTotal, decimal tolerance = 0.005m)
    {
        var ls = lines.ToArray();
        if (!invoiceTotal.IsKnown) return new(ReconStatus.NotEstablished, null, "Invoice total unknown.");
        if (ls.Length == 0 || ls.Any(l => !l.Cost.IsKnown))
            return new(ReconStatus.NotEstablished, null, "Usage cost unknown for at least one line; unknown is not zero.");
        var sum = ls.Sum(l => l.Cost.Value!.Value);
        var diff = invoiceTotal.Value!.Value - sum;
        return Math.Abs(diff) <= tolerance
            ? new(ReconStatus.Matched, diff, "Usage lines equal invoice total.")
            : new(ReconStatus.Mismatch, diff, "Usage lines differ from invoice total.");
    }

    public static IReadOnlyList<string> OverlappingCategories(IEnumerable<UsageLine> lines) =>
        lines.GroupBy(l => l.Category).Where(g => g.Count() > 1 && g.Any(x => x.Category == "reasoning")).Select(g => g.Key).ToArray();
}
