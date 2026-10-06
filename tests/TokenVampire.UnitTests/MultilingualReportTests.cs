using System.Text;
using TokenVampire.Domain;
using TokenVampire.I18n;
using TokenVampire.Reporting;
using Xunit;

public class MultilingualReportTests
{
    public static TheoryData<string> Tags => new() { "en", "ar", "fr", "es", "ur", "hi", "zh", "ja" };

    private static CaseLedger Sample()
    {
        var l = new CaseLedger();
        l.Add(new Charge("p01", ChargeKind.CreditTopUp, Money.Of(11.5m, "USD"), new(2026, 9, 7), "e1"));
        l.AddGap("g1", "No complete usage export");
        return l;
    }

    [Theory]
    [MemberData(nameof(Tags))]
    public void Locale_catalog_is_complete(string tag) => Assert.Empty(Catalog.Builtin().MissingKeys(tag));

    [Theory]
    [MemberData(nameof(Tags))]
    public void Report_has_no_missing_keys_and_is_nfc(string tag)
    {
        var r = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), tag);
        Assert.DoesNotContain("!report.", r, StringComparison.Ordinal);
        Assert.True(r.IsNormalized(NormalizationForm.FormC));
    }

    [Theory]
    [InlineData("fr", "RAPPORT DE DOSSIER")]
    [InlineData("es", "INFORME DEL CASO")]
    [InlineData("zh", "案件报告")]
    [InlineData("ja", "ケースレポート")]
    [InlineData("hi", "मामले की रिपोर्ट")]
    public void Report_uses_locale_title(string tag, string title) =>
        Assert.Contains(title, CaseReport.Render(Sample(), "USD", Catalog.Builtin(), tag), StringComparison.Ordinal);

    [Fact]
    public void Urdu_is_rtl_and_isolates_numbers()
    {
        var r = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), "ur-PK");
        Assert.Equal(TextDirection.Rtl, Locale.DirectionOf("ur-PK"));
        Assert.Contains("\u206811.50\u2069", r, StringComparison.Ordinal);
    }

    [Fact]
    public void Ltr_locales_have_no_bidi_controls()
    {
        foreach (var t in new[] { "fr", "es", "hi", "zh", "ja" })
        {
            Assert.False(UnicodeText.ContainsBidiControl(CaseReport.Render(Sample(), "USD", Catalog.Builtin(), t)));
        }
    }

    [Fact]
    public void Utf8_bytes_have_no_bom_and_round_trip()
    {
        var text = CaseReport.Render(Sample(), "USD", Catalog.Builtin(), "ar");
        var b = ReportFile.ToUtf8Bytes(text);
        Assert.False(b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF);
        Assert.Equal(UnicodeText.Nfc(text), Encoding.UTF8.GetString(b));
    }

    [Fact]
    public void Utf8_converts_crlf_to_lf()
    {
        Assert.Equal("a\nb", Encoding.UTF8.GetString(ReportFile.ToUtf8Bytes("a\r\nb")));
    }
}
