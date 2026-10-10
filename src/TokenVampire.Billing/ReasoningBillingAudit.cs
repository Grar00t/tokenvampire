using TokenVampire.Domain;

namespace TokenVampire.Billing;

/// <summary>
/// Provider-reported output counters may include thinking tokens or exclude them.
/// Accounting semantics must be supplied from a dated, model-specific source.
/// </summary>
public enum ReasoningAccountingMode
{
    Unknown = 0,
    InclusiveOutput,
    AdditiveOutput
}

public enum ReasoningEstimateState
{
    NotEstablished = 0,
    RateBasedEstimate,
    ScenarioOnly
}

public sealed record ReasoningBillingInput(
    Measured<long> ReportedOutputTokens,
    Measured<long> ReasoningTokens,
    ReasoningAccountingMode AccountingMode,
    Measured<decimal> OutputRatePerMillion,
    string Currency);

/// <summary>
/// All monetary amounts here are *computed output-rate portions*, never paid or
/// invoiced amounts. The reasoning portion is already contained in the output
/// estimate; do not add the two amounts together.
/// </summary>
public sealed record ReasoningBillingReview(
    ReasoningEstimateState OutputState,
    ReasoningEstimateState ReasoningState,
    long? EstimatedBillableOutputTokens,
    decimal? EstimatedOutputCost,
    decimal? ReasoningPortionOfOutputCost,
    string Currency,
    string Explanation);

public static class ReasoningBillingAudit
{
    public static ReasoningBillingReview Calculate(ReasoningBillingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var currency = input.Currency is { Length: 3 } && input.Currency.All(char.IsAsciiLetter)
            ? input.Currency.ToUpperInvariant()
            : "UNKNOWN";

        ReasoningBillingReview Unknown(string why) =>
            new(ReasoningEstimateState.NotEstablished, ReasoningEstimateState.NotEstablished,
                null, null, null, currency, why);

        if (currency == "UNKNOWN")
            return Unknown("A three-letter currency is required.");
        if (input.AccountingMode is not (ReasoningAccountingMode.InclusiveOutput or ReasoningAccountingMode.AdditiveOutput))
            return Unknown("Provider/model output-accounting convention is unknown.");

        if (!input.ReportedOutputTokens.IsKnown)
            return Unknown("Reported output token count is missing.");
        var reported = input.ReportedOutputTokens.Value!.Value;
        if (reported < 0)
            return Unknown("Reported output tokens cannot be negative.");

        if (!input.OutputRatePerMillion.IsKnown)
            return Unknown("Dated model-specific output rate is missing.");
        var rate = input.OutputRatePerMillion.Value!.Value;
        if (rate < 0m)
            return Unknown("Output rate cannot be negative.");

        long? reasoning = null;
        if (input.ReasoningTokens.IsKnown)
        {
            reasoning = input.ReasoningTokens.Value!.Value;
            if (reasoning.Value < 0)
                return Unknown("Reasoning tokens cannot be negative.");
        }
        if (input.AccountingMode == ReasoningAccountingMode.InclusiveOutput &&
            reasoning > reported)
            return Unknown("Reasoning tokens cannot exceed an inclusive output total.");
        if (input.AccountingMode == ReasoningAccountingMode.AdditiveOutput && reasoning is null)
            return Unknown("Additive accounting requires an explicit reasoning count; missing is not zero.");

        try
        {
            var total = input.AccountingMode == ReasoningAccountingMode.AdditiveOutput
                ? checked(reported + reasoning!.Value)
                : reported;
            var outputCost = checked(total * rate) / 1_000_000m;
            var reasoningCost = reasoning is not null
                ? checked(reasoning.Value * rate) / 1_000_000m
                : (decimal?)null;

            var outputBasedOnMeasuredValues =
                input.ReportedOutputTokens.IsMeasured && input.OutputRatePerMillion.IsMeasured &&
                (input.AccountingMode == ReasoningAccountingMode.InclusiveOutput || input.ReasoningTokens.IsMeasured);
            var reasoningBasedOnMeasuredValues =
                reasoning.HasValue && input.ReasoningTokens.IsMeasured && input.OutputRatePerMillion.IsMeasured;

            return new(
                outputBasedOnMeasuredValues ? ReasoningEstimateState.RateBasedEstimate : ReasoningEstimateState.ScenarioOnly,
                !reasoning.HasValue ? ReasoningEstimateState.NotEstablished
                    : reasoningBasedOnMeasuredValues ? ReasoningEstimateState.RateBasedEstimate : ReasoningEstimateState.ScenarioOnly,
                total, outputCost, reasoningCost, currency,
                "Output-rate estimate only; not an invoice or proof of a vendor charge. " +
                "The reasoning portion is included in the estimated output total, never an additional charge.");
        }
        catch (OverflowException)
        {
            return Unknown("Usage or estimated cost exceeds supported numeric range.");
        }
    }
}
