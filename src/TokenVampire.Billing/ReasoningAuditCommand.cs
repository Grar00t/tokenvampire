using System.Security.Cryptography;
using System.Text.Json;
using TokenVampire.Domain;

namespace TokenVampire.Billing;

/// <summary>Strict local-only reasoning cost estimation. No API calls, invoice assertions or secret collection.</summary>
public static class ReasoningAuditCommand
{
    public const int MaxBytes = 32 * 1024;
    public const string Usage = "usage: reasoning-audit --input <local-scenario.json>";

    public static int Run(string[] args, Func<string, byte[]> read, TextWriter stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);

        if (args.Length != 2 || args[0] != "--input" || string.IsNullOrWhiteSpace(args[1]))
        {
            stderr.WriteLine(Usage);
            return 2;
        }

        byte[] input;
        try
        {
            input = read(args[1]);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("Cannot read local reasoning audit input.");
            return 2;
        }

        if (!TryParse(input, out var provider, out var model, out var values))
        {
            stderr.WriteLine("Invalid or unsafe reasoning audit input; see docs/reasoning-billing.md.");
            return 2;
        }

        var review = ReasoningBillingAudit.Calculate(values!);
        var output = new
        {
            schemaVersion = 1,
            provider,
            model,
            inputSha256 = Convert.ToHexStringLower(SHA256.HashData(input)),
            outputState = review.OutputState.ToString(),
            reasoningState = review.ReasoningState.ToString(),
            accountedOutputTokens = review.EstimatedBillableOutputTokens,
            estimatedOutputCost = review.EstimatedOutputCost,
            reasoningPortionAlreadyIncluded = review.ReasoningPortionOfOutputCost,
            currency = review.Currency,
            invoiceVerified = false,
            warning = review.Explanation
        };
        stdout.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private static bool TryParse(byte[] input, out string provider, out string model, out ReasoningBillingInput? values)
    {
        provider = string.Empty;
        model = string.Empty;
        values = null;
        if (input.Length is 0 or > MaxBytes) return false;

        try
        {
            using var json = JsonDocument.Parse(input, new JsonDocumentOptions { MaxDepth = 8 });
            var root = json.RootElement;
            if (!Properties(root, ["schemaVersion", "provider", "model", "currency", "accountingMode",
                                   "reportedOutputTokens", "reasoningTokens", "outputRatePerMillion"]))
                return false;
            if (!root.TryGetProperty("schemaVersion", out var version) || !version.TryGetInt32(out var v) || v != 1)
                return false;
            if (!TextField(root, "provider", 32, out provider) || !TextField(root, "model", 64, out model) ||
                !TextField(root, "currency", 3, out var currency) || currency.Length != 3 ||
                !currency.All(char.IsAsciiLetter))
                return false;
            if (!root.TryGetProperty("accountingMode", out var accounting) ||
                accounting.ValueKind != JsonValueKind.String)
                return false;
            var mode = accounting.GetString() switch
            {
                "inclusiveOutput" => ReasoningAccountingMode.InclusiveOutput,
                "additiveOutput" => ReasoningAccountingMode.AdditiveOutput,
                "unknown" => ReasoningAccountingMode.Unknown,
                _ => (ReasoningAccountingMode)(-1)
            };
            if (!Enum.IsDefined(mode)) return false;
            if (!TokenCount(root, "reportedOutputTokens", out var output) ||
                !TokenCount(root, "reasoningTokens", out var reasoning) ||
                !TokenRate(root, "outputRatePerMillion", out var rate))
                return false;
            values = new(output, reasoning, mode, rate, currency);
            return true;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or OverflowException)
        {
            return false;
        }
    }

    private static bool Properties(JsonElement element, string[] allowed)
    {
        if (element.ValueKind != JsonValueKind.Object) return false;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!seen.Add(property.Name) || !allowed.Contains(property.Name, StringComparer.Ordinal))
                return false;
        }
        return true;
    }

    private static bool TextField(JsonElement element, string name, int max, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
            return false;
        var text = property.GetString();
        if (string.IsNullOrWhiteSpace(text) || text.Length > max ||
            !text.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or '/' or ':'))
            return false;
        value = text;
        return true;
    }

    private static bool TokenCount(JsonElement root, string name, out Measured<long> result)
    {
        result = Measured<long>.Unknown;
        if (!root.TryGetProperty(name, out var element)) return true;
        if (!Properties(element, ["value", "provenance"]) || !EvidenceState(element, out var status))
            return false;
        if (status == Provenance.Unknown) return !element.TryGetProperty("value", out _);
        if (!element.TryGetProperty("value", out var value) ||
            value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var n) || n < 0)
            return false;
        result = status == Provenance.Measured ? Measured<long>.Of(n) : Measured<long>.Assume(n);
        return true;
    }

    private static bool TokenRate(JsonElement root, string name, out Measured<decimal> result)
    {
        result = Measured<decimal>.Unknown;
        if (!root.TryGetProperty(name, out var element)) return true;
        if (!Properties(element, ["value", "provenance"]) || !EvidenceState(element, out var status))
            return false;
        if (status == Provenance.Unknown) return !element.TryGetProperty("value", out _);
        if (!element.TryGetProperty("value", out var value) ||
            value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var n) || n < 0m)
            return false;
        result = status == Provenance.Measured ? Measured<decimal>.Of(n) : Measured<decimal>.Assume(n);
        return true;
    }

    private static bool EvidenceState(JsonElement element, out Provenance status)
    {
        status = Provenance.Unknown;
        if (!element.TryGetProperty("provenance", out var source) || source.ValueKind != JsonValueKind.String)
            return false;
        switch (source.GetString())
        {
            case "measured": status = Provenance.Measured; return true;
            case "assumed": status = Provenance.Assumed; return true;
            case "unknown": return true;
            default: return false;
        }
    }
}
