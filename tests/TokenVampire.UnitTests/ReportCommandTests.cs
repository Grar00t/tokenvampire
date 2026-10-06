using System.Text;
using TokenVampire.Reporting;
using Xunit;

public class ReportCommandTests
{
    private const string Good = """
        {"items":[{"id":"p01","kind":"CreditTopUp","amount":11.5,"currency":"USD","date":"2026-09-07","evidence":"e1"}],
         "gaps":{"g1":"No complete usage export"}}
        """;

    private const string Dup = """
        {"items":[{"id":"p01","kind":"CreditTopUp","amount":1,"currency":"USD","date":"2026-09-07","evidence":"e1"},
                  {"id":"p01","kind":"Refund","amount":1,"currency":"USD","date":"2026-09-08","evidence":"e2"}]}
        """;

    private const string NumericKind = """
        {"items":[{"id":"p01","kind":"1","amount":1,"currency":"USD","date":"2026-09-07","evidence":"e1"}]}
        """;

    private static (int Code, byte[] Out, string Err, Dictionary<string, byte[]> Files) Run(string json, params string[] args)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        using var so = new MemoryStream();
        var se = new StringWriter();
        var code = ReportCommand.Run(
            args,
            p => p == "l.json" ? Encoding.UTF8.GetBytes(json) : throw new FileNotFoundException(p),
            (p, b) => files[p] = b,
            so,
            se);
        return (code, so.ToArray(), se.ToString(), files);
    }

    [Fact]
    public void Valid_ledger_renders_english_without_bom()
    {
        var r = Run(Good, "--ledger", "l.json");
        Assert.Equal(0, r.Code);
        Assert.False(r.Out.Length >= 3 && r.Out[0] == 0xEF && r.Out[1] == 0xBB && r.Out[2] == 0xBF);
        var text = Encoding.UTF8.GetString(r.Out);
        Assert.Contains("CASE REPORT (USD)", text, StringComparison.Ordinal);
        Assert.Contains("11.50", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Arabic_locale_renders_arabic_title()
    {
        var r = Run(Good, "--ledger", "l.json", "--locale", "ar-SA");
        Assert.Equal(0, r.Code);
        Assert.Contains("تقرير الحالة", Encoding.UTF8.GetString(r.Out), StringComparison.Ordinal);
    }

    [Fact]
    public void Out_option_writes_file_and_leaves_stdout_empty()
    {
        var r = Run(Good, "--ledger", "l.json", "--out", "o.md");
        Assert.Equal(0, r.Code);
        Assert.Empty(r.Out);
        Assert.True(r.Files.ContainsKey("o.md"));
    }

    [Fact]
    public void Unknown_locale_warns_and_falls_back()
    {
        var r = Run(Good, "--ledger", "l.json", "--locale", "xx");
        Assert.Equal(0, r.Code);
        Assert.Contains("fallback", r.Err, StringComparison.Ordinal);
        Assert.Contains("CASE REPORT", Encoding.UTF8.GetString(r.Out), StringComparison.Ordinal);
    }

    [Fact]
    public void Input_with_bom_is_accepted()
    {
        Assert.Equal(0, Run("\uFEFF" + Good, "--ledger", "l.json").Code);
    }

    [Fact]
    public void Duplicate_item_id_is_rejected()
    {
        var r = Run(Dup, "--ledger", "l.json");
        Assert.Equal(2, r.Code);
        Assert.Contains("duplicate", r.Err, StringComparison.Ordinal);
        Assert.Empty(r.Out);
    }

    [Fact]
    public void Numeric_kind_is_rejected()
    {
        var r = Run(NumericKind, "--ledger", "l.json");
        Assert.Equal(2, r.Code);
        Assert.Contains("unknown kind", r.Err, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_file_unknown_option_and_bad_json_exit_2()
    {
        Assert.Equal(2, Run(Good, "--ledger", "nope.json").Code);
        Assert.Equal(2, Run(Good, "--bogus", "x").Code);
        Assert.Equal(2, Run(Good, "--ledger").Code);
        Assert.Equal(2, Run("{not json", "--ledger", "l.json").Code);
        Assert.Equal(2, Run(Good, "--ledger", "l.json", "--currency", "US").Code);
    }

    [Fact]
    public void Bidi_override_in_gap_is_stripped_end_to_end()
    {
        var json = "{\"gaps\":{\"g2\":\"abc\\u202Edef\"}}";
        var text = Encoding.UTF8.GetString(Run(json, "--ledger", "l.json").Out);
        Assert.Contains("abcdef", text, StringComparison.Ordinal);
        Assert.False(TokenVampire.I18n.UnicodeText.ContainsBidiControl(text));
    }
}
