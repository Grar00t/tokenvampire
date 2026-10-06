namespace TokenVampire.I18n;

public sealed class Catalog
{
    private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.Ordinal);

    public void Set(string locale, string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        var loc = Locale.Canonical(locale);
        if (!_data.TryGetValue(loc, out var map))
        {
            map = new Dictionary<string, string>(StringComparer.Ordinal);
            _data[loc] = map;
        }
        map[key] = UnicodeText.Nfc(value);
    }

    public bool TryGet(string key, string locale, out string value)
    {
        foreach (var loc in Locale.Chain(locale))
        {
            if (_data.TryGetValue(loc, out var map) && map.TryGetValue(key, out var v))
            {
                value = v;
                return true;
            }
        }
        value = string.Empty;
        return false;
    }

    public string Get(string key, string locale) => TryGet(key, locale, out var v) ? v : "!" + key + "!";

    public IReadOnlyList<string> MissingKeys(string locale)
    {
        var loc = Locale.Canonical(locale);
        var have = _data.TryGetValue(loc, out var m) ? m : new Dictionary<string, string>(StringComparer.Ordinal);
        if (!_data.TryGetValue("en", out var en))
        {
            return [];
        }
        return en.Keys.Where(k => !have.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
    }

    public static Catalog Builtin()
    {
        var c = new Catalog();
        c.Set("en", "report.title", "CASE REPORT");
        c.Set("en", "report.funded", "Funded (credit top-ups, not spend)");
        c.Set("en", "report.usage", "Billed usage (consumption)");
        c.Set("en", "report.not_measured", "NOT MEASURED");
        c.Set("en", "report.gaps", "Declared gaps");
        c.Set("en", "report.boundary", "A missing figure is unknown, not zero. This is not a legal refund entitlement.");
        c.Set("ar", "report.title", "تقرير الحالة");
        c.Set("ar", "report.funded", "التمويل (شحن الرصيد، وليس إنفاقاً)");
        c.Set("ar", "report.usage", "الاستهلاك المفوتر");
        c.Set("ar", "report.not_measured", "غير مقاس");
        c.Set("ar", "report.gaps", "الفجوات المعلنة");
        c.Set("ar", "report.boundary", "الرقم المفقود مجهول وليس صفراً. وهذا ليس استحقاقاً قانونياً لاسترداد.");
        return c;
    }
}
