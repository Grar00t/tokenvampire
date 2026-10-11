using System.Text.Json;
using TokenVampire.Domain;

namespace TokenVampire.Parsing;

public sealed record TokenEstimate(Measured<long> Tokens, string Method, long InputCharacters, long? PlanLimit,
    Measured<long> RemainingEstimate);

public static class LocalTokenEstimator
{
    private const int MaxBytes = 16 * 1024 * 1024;
    private const int MaxDepth = 32;

    // Deliberately not vendor-tokenizer output. Only a bounded, transparent character heuristic.
    public static TokenEstimate FromJson(ReadOnlyMemory<byte> data, long? planLimit = null)
    {
        if (planLimit < 0) throw new ArgumentOutOfRangeException(nameof(planLimit));
        if (data.Length == 0 || data.Length > MaxBytes)
            return Unknown(planLimit);
        try
        {
            using var doc = JsonDocument.Parse(data, new JsonDocumentOptions { MaxDepth = MaxDepth });
            long characters = 0;
            var extracted = false;
            Walk(doc.RootElement, ref characters, ref extracted);
            if (!extracted) return Unknown(planLimit);
            // Ceiling per message, not exact tokenization; no official reconciliation may use it.
            var estimated = (characters + 3) / 4;
            var remaining = planLimit.HasValue
                ? Measured<long>.Assume(Math.Max(0, planLimit.Value - estimated))
                : Measured<long>.Unknown;
            return new(Measured<long>.Assume(estimated), "heuristic/utf16-chars-div4/v1",
                characters, planLimit, remaining);
        }
        catch (Exception ex) when (ex is JsonException or OverflowException)
        {
            return Unknown(planLimit);
        }
    }

    private static TokenEstimate Unknown(long? planLimit) =>
        new(Measured<long>.Unknown, "unsupported-or-invalid", 0, planLimit, Measured<long>.Unknown);

    private static void Walk(JsonElement node, ref long characters, ref bool extracted)
    {
        switch (node.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var field in node.EnumerateObject())
                {
                    // HAR request/response content.text, export message.content, and message.parts.
                    if (field.Name is "content" or "text" or "parts" or "messages")
                        Walk(field.Value, ref characters, ref extracted);
                    else if (field.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        Walk(field.Value, ref characters, ref extracted);
                }
                break;
            case JsonValueKind.Array:
                foreach (var child in node.EnumerateArray()) Walk(child, ref characters, ref extracted);
                break;
            case JsonValueKind.String:
                extracted = true;
                characters = checked(characters + node.GetString()!.Length);
                break;
        }
    }
}
