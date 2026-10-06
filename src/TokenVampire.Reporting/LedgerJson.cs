using System.Globalization;
using System.Text.Json;
using TokenVampire.Domain;

namespace TokenVampire.Reporting;

public static class LedgerJson
{
    public static bool TryParse(byte[] utf8, out CaseLedger ledger, out string error)
    {
        ArgumentNullException.ThrowIfNull(utf8);
        ledger = new CaseLedger();
        error = string.Empty;
        var mem = utf8.AsMemory();
        if (mem.Length >= 3 && utf8[0] == 0xEF && utf8[1] == 0xBB && utf8[2] == 0xBF)
        {
            mem = mem[3..];
        }
        try
        {
            using var doc = JsonDocument.Parse(mem, new JsonDocumentOptions { MaxDepth = 8 });
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Fail(out ledger, out error, "root must be an object");
            }
            if (root.TryGetProperty("items", out var items))
            {
                if (items.ValueKind != JsonValueKind.Array)
                {
                    return Fail(out ledger, out error, "items must be an array");
                }
                var n = 0;
                foreach (var it in items.EnumerateArray())
                {
                    n++;
                    var charge = Item(it, out var why);
                    if (charge is null)
                    {
                        return Fail(out ledger, out error, "items[" + n.ToString(CultureInfo.InvariantCulture) + "]: " + why);
                    }
                    if (!ledger.Add(charge))
                    {
                        return Fail(out ledger, out error, "duplicate item id: " + charge.Id);
                    }
                }
            }
            if (root.TryGetProperty("gaps", out var gaps))
            {
                if (gaps.ValueKind != JsonValueKind.Object)
                {
                    return Fail(out ledger, out error, "gaps must be an object");
                }
                foreach (var p in gaps.EnumerateObject())
                {
                    if (p.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(p.Value.GetString()))
                    {
                        return Fail(out ledger, out error, "gap '" + p.Name + "' must be a non-empty string");
                    }
                    if (!ledger.AddGap(p.Name, p.Value.GetString()!))
                    {
                        return Fail(out ledger, out error, "duplicate gap id: " + p.Name);
                    }
                }
            }
            return true;
        }
        catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException)
        {
            return Fail(out ledger, out error, "invalid ledger: " + e.Message);
        }
    }

    private static bool Fail(out CaseLedger ledger, out string error, string message)
    {
        ledger = new CaseLedger();
        error = message;
        return false;
    }

    private static string? Str(JsonElement o, string name) =>
        o.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static Charge? Item(JsonElement it, out string why)
    {
        why = string.Empty;
        if (it.ValueKind != JsonValueKind.Object)
        {
            why = "must be an object";
            return null;
        }
        var id = Str(it, "id");
        var kind = Str(it, "kind");
        var cur = Str(it, "currency");
        var date = Str(it, "date");
        var ev = Str(it, "evidence");
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(cur)
            || string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(ev))
        {
            why = "id, kind, currency, date, evidence must be non-empty strings";
            return null;
        }
        if (!it.TryGetProperty("amount", out var a) || a.ValueKind != JsonValueKind.Number || !a.TryGetDecimal(out var amount))
        {
            why = "amount must be a decimal number";
            return null;
        }
        if (!kind.All(char.IsLetter) || !Enum.TryParse<ChargeKind>(kind, false, out var k) || !Enum.IsDefined(k))
        {
            why = "unknown kind '" + kind + "'";
            return null;
        }
        if (cur.Length != 3 || !cur.All(char.IsAsciiLetter))
        {
            why = "currency must be a 3-letter ISO 4217 code";
            return null;
        }
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            why = "date must be yyyy-MM-dd";
            return null;
        }
        return new Charge(id, k, Money.Of(amount, cur), d, ev);
    }
}
