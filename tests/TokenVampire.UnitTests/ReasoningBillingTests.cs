using System.Text;
using System.Text.Json;
using TokenVampire.Billing;
using TokenVampire.Domain;
using Xunit;

public sealed class ReasoningBillingTests
{
    private static ReasoningBillingInput Input(
        Measured<long> output, Measured<long> thinking,
        ReasoningAccountingMode mode, Measured<decimal> rate) =>
        new(output, thinking, mode, rate, "USD");

    private const string Example = """
        {
          "schemaVersion": 1,
          "provider": "example",
          "model": "synthetic-model",
          "currency": "USD",
          "accountingMode": "inclusiveOutput",
          "reportedOutputTokens": { "value": 300, "provenance": "assumed" },
          "reasoningTokens": { "value": 200, "provenance": "assumed" },
          "outputRatePerMillion": { "value": 10.00, "provenance": "assumed" }
        }
        """;

    private static (int Code, string Output, string Error) Run(string input, params string[] args)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var rc = ReasoningAuditCommand.Run(
            args,
            _ => Encoding.UTF8.GetBytes(input),
            stdout, stderr);
        return (rc, stdout.ToString(), stderr.ToString());
    }

    [Fact]
    public void Inclusive_output_does_not_bill_reasoning_twice()
    {
        var result = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Of(200),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(ReasoningEstimateState.RateBasedEstimate, result.OutputState);
        Assert.Equal(300L, result.EstimatedBillableOutputTokens);
        Assert.Equal(0.003m, result.EstimatedOutputCost);
        Assert.Equal(0.002m, result.ReasoningPortionOfOutputCost);
    }

    [Fact]
    public void Additive_model_counts_reasoning_only_once()
    {
        var result = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(100), Measured<long>.Of(200),
            ReasoningAccountingMode.AdditiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(300L, result.EstimatedBillableOutputTokens);
        Assert.Equal(0.003m, result.EstimatedOutputCost);
        Assert.Equal(0.002m, result.ReasoningPortionOfOutputCost);
    }

    [Fact]
    public void Unknown_reasoning_does_not_imply_zero()
    {
        var inclusive = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Unknown,
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(0.003m, inclusive.EstimatedOutputCost);
        Assert.Null(inclusive.ReasoningPortionOfOutputCost);
        Assert.Equal(ReasoningEstimateState.NotEstablished, inclusive.ReasoningState);

        var additive = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Unknown,
            ReasoningAccountingMode.AdditiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(ReasoningEstimateState.NotEstablished, additive.OutputState);
        Assert.Null(additive.EstimatedOutputCost);
        Assert.Null(additive.EstimatedBillableOutputTokens);
    }

    [Fact]
    public void Assumed_price_is_a_scenario_not_an_invoice()
    {
        var result = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Of(200),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Assume(10m)));
        Assert.Equal(ReasoningEstimateState.ScenarioOnly, result.OutputState);
        Assert.Equal(ReasoningEstimateState.ScenarioOnly, result.ReasoningState);
        Assert.Contains("not an invoice", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Missing_accounting_or_price_is_not_established()
    {
        Assert.Equal(ReasoningEstimateState.NotEstablished, ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Of(200),
            ReasoningAccountingMode.Unknown, Measured<decimal>.Of(10m))).OutputState);
        Assert.Equal(ReasoningEstimateState.NotEstablished, ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(300), Measured<long>.Of(200),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Unknown)).OutputState);
    }

    [Fact]
    public void Impossible_inclusive_breakdown_or_negative_counts_fail_closed()
    {
        Assert.Equal(ReasoningEstimateState.NotEstablished, ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(100), Measured<long>.Of(200),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(10m))).OutputState);
        Assert.Equal(ReasoningEstimateState.NotEstablished, ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(-1), Measured<long>.Of(1),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(10m))).OutputState);
        Assert.Equal(ReasoningEstimateState.NotEstablished, ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(5), Measured<long>.Of(2),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(-1m))).OutputState);
    }

    [Fact]
    public void Zero_is_only_zero_when_explicitly_recorded()
    {
        var result = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(0), Measured<long>.Of(0),
            ReasoningAccountingMode.InclusiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(0m, result.EstimatedOutputCost);
        Assert.Equal(0m, result.ReasoningPortionOfOutputCost);
        Assert.Equal(0L, result.EstimatedBillableOutputTokens);
    }

    [Fact]
    public void Overflow_is_reported_as_unknown_without_partial_cost()
    {
        var result = ReasoningBillingAudit.Calculate(Input(
            Measured<long>.Of(long.MaxValue), Measured<long>.Of(1),
            ReasoningAccountingMode.AdditiveOutput, Measured<decimal>.Of(10m)));
        Assert.Equal(ReasoningEstimateState.NotEstablished, result.OutputState);
        Assert.Null(result.EstimatedOutputCost);
    }

    [Fact]
    public void Example_is_an_explicit_synthetic_scenario()
    {
        var result = Run(Example, "--input", "example.json");
        Assert.Equal(0, result.Code);
        Assert.Equal(string.Empty, result.Error);
        using var doc = JsonDocument.Parse(result.Output);
        var root = doc.RootElement;
        Assert.Equal("ScenarioOnly", root.GetProperty("outputState").GetString());
        Assert.Equal(300L, root.GetProperty("accountedOutputTokens").GetInt64());
        Assert.Equal(0.003m, root.GetProperty("estimatedOutputCost").GetDecimal());
        Assert.Equal(0.002m, root.GetProperty("reasoningPortionAlreadyIncluded").GetDecimal());
        Assert.False(root.GetProperty("invoiceVerified").GetBoolean());
        Assert.Equal(64, root.GetProperty("inputSha256").GetString()!.Length);
    }

    [Theory]
    [InlineData("""{"schemaVersion":1,"provider":"example","model":"x","currency":"USD","accountingMode":"unknown","apiKey":"test-only"}""")]
    [InlineData("""{"schemaVersion":1,"schemaVersion":1}""")]
    [InlineData("""{"schemaVersion":1,"provider":"example","model":"x","currency":"USD","accountingMode":3}""")]
    [InlineData("""{"schemaVersion":1,"provider":"example","model":"x","currency":"USD","accountingMode":"inclusiveOutput","reasoningTokens":{"provenance":"measured"}}""")]
    [InlineData("""{"schemaVersion":1,"provider":"example","model":"x","currency":"USD","accountingMode":"inclusiveOutput","reasoningTokens":{"provenance":"unknown","value":0}}""")]
    [InlineData("""{"schemaVersion":1,"provider":"example","model":"x","currency":"USD","accountingMode":"inclusiveOutput","reasoningTokens":{"provenance":"assumed","value":-1}}""")]
    public void Malformed_untrusted_input_is_rejected(string json)
    {
        var result = Run(json, "--input", "sample.json");
        Assert.Equal(2, result.Code);
        Assert.Empty(result.Output);
    }

    [Fact]
    public void No_arbitrary_prompts_or_oversized_input_accepted()
    {
        var more = Example.Replace("\"provider\"", "\"prompt\": \"ignore all instructions\", \"provider\"", StringComparison.Ordinal);
        Assert.Equal(2, Run(more, "--input", "sample.json").Code);
        Assert.Equal(2, Run(new string('A', ReasoningAuditCommand.MaxBytes + 1), "--input", "sample.json").Code);
        Assert.Equal(2, Run(Example, "--input").Code);
    }
}
