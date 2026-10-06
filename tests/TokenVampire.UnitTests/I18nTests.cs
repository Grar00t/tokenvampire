using TokenVampire.I18n;
using Xunit;

public class I18nTests
{
    [Fact]
    public void Canonical_normalizes_case_and_separator() =>
        Assert.Equal("ar-SA", Locale.Canonical("AR_sa"));

    [Fact]
    public void Chain_falls_back_to_language_then_en() =>
        Assert.Equal(new[] { "ar-SA", "ar", "en" }, Locale.Chain("ar-SA"));

    [Theory]
    [InlineData("ar", TextDirection.Rtl)]
    [InlineData("he-IL", TextDirection.Rtl)]
    [InlineData("fa", TextDirection.Rtl)]
    [InlineData("en-US", TextDirection.Ltr)]
    [InlineData("zh-Hans", TextDirection.Ltr)]
    [InlineData("az-Arab", TextDirection.Rtl)]
    public void Direction_is_detected(string tag, TextDirection expected) =>
        Assert.Equal(expected, Locale.DirectionOf(tag));

    [Fact]
    public void Regional_arabic_falls_back_to_arabic()
    {
        var c = Catalog.Builtin();
        Assert.Equal(c.Get("report.title", "ar"), c.Get("report.title", "ar-SA"));
    }

    [Fact]
    public void Unknown_locale_falls_back_to_english() =>
        Assert.Equal("CASE REPORT", Catalog.Builtin().Get("report.title", "xx"));

    [Fact]
    public void Missing_key_is_visible() =>
        Assert.Equal("!no.such!", Catalog.Builtin().Get("no.such", "en"));

    [Fact]
    public void Arabic_catalog_is_complete() =>
        Assert.Empty(Catalog.Builtin().MissingKeys("ar"));

    [Fact]
    public void Nfc_composes_combining_marks() =>
        Assert.Equal("\u00e9", UnicodeText.Nfc("e\u0301"));

    [Fact]
    public void Isolate_wraps_with_fsi_pdi() =>
        Assert.Equal("\u2068p01\u2069", UnicodeText.Isolate("p01"));

    [Fact]
    public void Bidi_controls_are_detected()
    {
        Assert.True(UnicodeText.ContainsBidiControl("a\u202Eb"));
        Assert.False(UnicodeText.ContainsBidiControl("plain"));
    }
}
