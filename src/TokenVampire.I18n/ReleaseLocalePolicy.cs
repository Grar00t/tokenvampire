namespace TokenVampire.I18n;

public static class ReleaseLocalePolicy
{
    private static readonly IReadOnlyList<string> Blocking =
        Array.AsReadOnly(new[] { "en", "ar" });

    public static IReadOnlyList<string> BlockingLocales => Blocking;

    public static void Validate(Catalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        foreach (var locale in Blocking)
        {
            if (!catalog.HasLocale(locale))
            {
                throw new InvalidOperationException(
                    $"Release-blocking locale '{locale}' has no catalog entries.");
            }

            var missing = catalog.MissingKeys(locale);
            if (missing.Count != 0)
            {
                throw new InvalidOperationException(
                    $"Release-blocking locale '{locale}' is missing: {string.Join(", ", missing)}");
            }
        }
    }
}
