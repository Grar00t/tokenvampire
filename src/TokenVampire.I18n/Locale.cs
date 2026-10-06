namespace TokenVampire.I18n;

public enum TextDirection { Ltr, Rtl }

public static class Locale
{
    private static readonly HashSet<string> RtlPrimary = new(StringComparer.Ordinal)
    {
        "ar", "he", "fa", "ur", "ps", "sd", "yi", "dv", "ug", "ckb"
    };

    private static readonly HashSet<string> RtlScripts = new(StringComparer.Ordinal)
    {
        "Arab", "Hebr", "Thaa", "Syrc", "Nkoo", "Adlm"
    };

    private static string[] Parts(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        var raw = tag.Trim().Replace('_', '-').Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (raw.Length == 0)
        {
            throw new ArgumentException("Empty locale tag", nameof(tag));
        }
        var parts = new string[raw.Length];
        for (var i = 0; i < raw.Length; i++)
        {
            var p = raw[i];
            parts[i] = i == 0 ? p.ToLowerInvariant()
                : p.Length == 2 ? p.ToUpperInvariant()
                : p.Length == 4 ? char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()
                : p.ToLowerInvariant();
        }
        return parts;
    }

    public static string Canonical(string tag) => string.Join('-', Parts(tag));

    public static IReadOnlyList<string> Chain(string tag)
    {
        var parts = Parts(tag);
        var chain = new List<string>();
        for (var n = parts.Length; n >= 1; n--)
        {
            chain.Add(string.Join('-', parts.Take(n)));
        }
        if (!chain.Contains("en"))
        {
            chain.Add("en");
        }
        return chain;
    }

    public static TextDirection DirectionOf(string tag)
    {
        var parts = Parts(tag);
        var script = parts.Skip(1).FirstOrDefault(p => p.Length == 4);
        if (script is not null)
        {
            return RtlScripts.Contains(script) ? TextDirection.Rtl : TextDirection.Ltr;
        }
        return RtlPrimary.Contains(parts[0]) ? TextDirection.Rtl : TextDirection.Ltr;
    }
}
