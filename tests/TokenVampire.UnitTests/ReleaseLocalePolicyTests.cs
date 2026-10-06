using TokenVampire.I18n;
using Xunit;

public class ReleaseLocalePolicyTests
{
    [Fact]
    public void Only_english_and_arabic_are_release_blocking() =>
        Assert.Equal(new[] { "en", "ar" }, ReleaseLocalePolicy.BlockingLocales);

    [Fact]
    public void Release_blocking_catalogs_are_complete() =>
        ReleaseLocalePolicy.Validate(Catalog.Builtin());
}
