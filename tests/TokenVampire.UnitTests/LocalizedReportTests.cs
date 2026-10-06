using TokenVampire.Domain;
using TokenVampire.I18n;
using TokenVampire.Reporting;
using Xunit;

public class LocalizedReportTests
{
    private static CaseLedger Sample()
    {
        var l = new CaseLedger();
        l.Add(new Charge("p01", ChargeKind.CreditTopUp, Money.Of(11.5m, "USD"), new(2026, 9, 7), "e1"));
        l.AddGap("g1", "No complete usage export");
        return l;
    }

    [Fact]
    public void Arabic_report_uses_arabic_text_and_not_measured()
    {
        var r = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), "ar-SA");
        Assert.Contains("تقرير الحالة", r);
        Assert.Contains("غير مقاس", r);
    }

    [Fact]
    public void Rtl_isolates_amounts_and_ids()
    {
        var r = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), "ar");
        Assert.Contains("\u206811.50\u2069", r, StringComparison.Ordinal);
        Assert.Contains("- \u2068g1\u2069: ", r, StringComparison.Ordinal);
    }

    [Fact]
    public void Ltr_report_has_no_isolation_marks()
    {
        var r = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), "en");
        Assert.False(UnicodeText.ContainsBidiControl(r));
    }

    [Fact]
    public void Bidi_override_in_user_text_is_stripped()
    {
        var l = new CaseLedger();
        l.AddGap("g2", "abc\u202Edef");
        var r = CaseReport.Render(l, "USD");
        Assert.Contains("abcdef", r, StringComparison.Ordinal);
        Assert.False(UnicodeText.ContainsBidiControl(r));
    }

    [Fact]
    public void No_missing_catalog_keys_in_report()
    {
        foreach (var loc in new[] { "en", "ar", "ja" })
        {
            Assert.DoesNotContain("!report.", CaseReport.Render(Sample(), "USD", Catalog.Builtin(), loc));
        }
    }
}
