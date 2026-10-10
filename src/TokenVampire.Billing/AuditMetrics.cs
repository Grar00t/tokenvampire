using TokenVampire.Domain;

namespace TokenVampire.Billing;

public sealed record Attempt(string Id, string RequestId, bool Retried, Measured<decimal> Tokens, Measured<decimal> Cost);
public sealed record CostAudit(
    Measured<decimal> RetryRate, Measured<decimal> TokenBurn,
    Measured<decimal> ReworkCost, Measured<decimal> ZeroInferenceOccurrences);

public static class AuditMetrics
{
    public static CostAudit Calculate(IEnumerable<Attempt> attempts, bool completeCapture)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        var xs = attempts.ToArray();
        if (!completeCapture || xs.Length == 0 ||
            xs.Any(x => string.IsNullOrWhiteSpace(x.Id) || string.IsNullOrWhiteSpace(x.RequestId)) ||
            xs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != xs.Length)
            return new(Measured<decimal>.Unknown, Measured<decimal>.Unknown,
                Measured<decimal>.Unknown, Measured<decimal>.Unknown);
        var retryRate = Measured<decimal>.Of((decimal)xs.Count(x => x.Retried) / xs.Length);
        var tokens = xs.All(x => x.Tokens.IsMeasured)
            ? Measured<decimal>.Of(xs.Sum(x => x.Tokens.Value!.Value))
            : Measured<decimal>.Unknown;
        var cost = xs.Where(x => x.Retried).All(x => x.Cost.IsMeasured)
            ? Measured<decimal>.Of(xs.Where(x => x.Retried).Sum(x => x.Cost.Value!.Value))
            : Measured<decimal>.Unknown;
        var zero = xs.All(x => x.Tokens.IsMeasured)
            ? Measured<decimal>.Of(xs.Count(x => x.Tokens.Value == 0m))
            : Measured<decimal>.Unknown;
        return new(retryRate, tokens, cost, zero);
    }
}
