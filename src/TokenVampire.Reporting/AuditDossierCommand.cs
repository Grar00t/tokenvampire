using System.Text;

namespace TokenVampire.Reporting;

public static class AuditDossierCommand
{
    public const string Usage = "usage: audit-case --input <redacted-dossier.json>";

    public static int Run(string[] args, Func<string, byte[]> read, TextWriter stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);
        if (args.Length != 2 || args[0] != "--input" || string.IsNullOrWhiteSpace(args[1]))
        {
            stderr.WriteLine(Usage);
            return 2;
        }

        byte[] bytes;
        try
        {
            bytes = read(args[1]);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("Could not read redacted case dossier.");
            return 2;
        }

        if (!AuditDossierCodec.TryRead(bytes, out var dossier, out var error))
        {
            stderr.WriteLine(error);
            return 2;
        }

        try
        {
            stdout.Write(AuditDossierCodec.RenderRedactedSummary(dossier!, bytes));
            stdout.Flush();
            return 0;
        }
        catch (Exception ex) when (ex is IOException or EncoderFallbackException)
        {
            stderr.WriteLine("Could not render redacted case dossier.");
            return 2;
        }
    }
}
