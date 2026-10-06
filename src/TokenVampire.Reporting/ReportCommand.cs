using System.Text;
using TokenVampire.Domain;
using TokenVampire.I18n;

namespace TokenVampire.Reporting;

public static class ReportCommand
{
    public const string Usage = "usage: report --ledger <file.json> [--locale <tag>] [--currency <ISO>] [--out <file>]";

    public static int Run(string[] args, Func<string, byte[]> read, Action<string, byte[]> write, Stream stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);
        string? ledgerPath = null;
        string? outPath = null;
        var locale = "en";
        var currency = "USD";
        for (var i = 0; i < args.Length; i += 2)
        {
            if (i + 1 >= args.Length)
            {
                stderr.WriteLine("missing value for " + args[i]);
                stderr.WriteLine(Usage);
                return 2;
            }
            var val = args[i + 1];
            switch (args[i])
            {
                case "--ledger": ledgerPath = val; break;
                case "--locale": locale = val; break;
                case "--currency": currency = val; break;
                case "--out": outPath = val; break;
                default:
                    stderr.WriteLine("unknown option " + args[i]);
                    stderr.WriteLine(Usage);
                    return 2;
            }
        }
        if (string.IsNullOrWhiteSpace(ledgerPath))
        {
            stderr.WriteLine("--ledger is required");
            stderr.WriteLine(Usage);
            return 2;
        }
        try
        {
            locale = Locale.Canonical(locale);
            currency = Money.Of(0m, currency).Currency;
        }
        catch (ArgumentException e)
        {
            stderr.WriteLine("invalid option: " + e.Message);
            return 2;
        }
        byte[] input;
        try
        {
            input = read(ledgerPath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("cannot read ledger: " + e.Message);
            return 2;
        }
        if (!LedgerJson.TryParse(input, out var ledger, out var error))
        {
            stderr.WriteLine(error);
            return 2;
        }
        var catalog = Catalog.Builtin();
        if (catalog.MissingKeys(locale).Count > 0)
        {
            stderr.WriteLine("warning: locale '" + locale + "' not available, using English fallback");
        }
        byte[] bytes;
        try
        {
            bytes = ReportFile.ToUtf8Bytes(CaseReport.Render(ledger, currency, catalog, locale));
        }
        catch (EncoderFallbackException)
        {
            stderr.WriteLine("ledger text is not valid Unicode");
            return 2;
        }
        try
        {
            if (outPath is null)
            {
                stdout.Write(bytes, 0, bytes.Length);
                stdout.Flush();
            }
            else
            {
                write(outPath, bytes);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("cannot write output: " + e.Message);
            return 2;
        }
        return 0;
    }
}
