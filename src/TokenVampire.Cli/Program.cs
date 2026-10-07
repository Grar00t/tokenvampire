using System.Globalization;
using TokenVampire.Infrastructure;
using TokenVampire.Reporting;

if (args.Length == 0)
{
    WriteUsage(Console.Error);
    return 2;
}

if (args[0] == "report")
{
    using var stdout = Console.OpenStandardOutput();
    return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);
}

if (args[0] == "verify-artifact")
    return VerifyArtifactCommand.Run(args[1..], Console.Out, Console.Error);

WriteUsage(Console.Error);
return 2;

static void WriteUsage(TextWriter stderr)
{
    stderr.WriteLine(ReportCommand.Usage);
    stderr.WriteLine(VerifyArtifactCommand.Usage);
}

public static class VerifyArtifactCommand
{
    public const string Usage =
        "usage: verify-artifact --path <file> [--sha256 <64-hex>] [--bytes <non-negative-int>]";

    public static int Run(string[] commandArgs, TextWriter stdout, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(commandArgs);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);

        string? path = null;
        string? expectedSha256 = null;
        long? expectedBytes = null;

        for (var i = 0; i < commandArgs.Length; i += 2)
        {
            if (i + 1 >= commandArgs.Length)
            {
                stderr.WriteLine("missing value for " + commandArgs[i]);
                stderr.WriteLine(Usage);
                return 2;
            }

            var value = commandArgs[i + 1];
            switch (commandArgs[i])
            {
                case "--path":
                    path = value;
                    break;
                case "--sha256":
                    expectedSha256 = value;
                    break;
                case "--bytes":
                    if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
                    {
                        stderr.WriteLine("--bytes must be a non-negative integer");
                        return 2;
                    }
                    expectedBytes = parsed;
                    break;
                default:
                    stderr.WriteLine("unknown option " + commandArgs[i]);
                    stderr.WriteLine(Usage);
                    return 2;
            }
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            stderr.WriteLine("--path is required");
            stderr.WriteLine(Usage);
            return 2;
        }

        var result = ArtifactVerifier.Verify(path, expectedSha256, expectedBytes);
        stdout.WriteLine("status=" + result.Status.ToString().ToUpperInvariant());
        stdout.WriteLine("path=" + result.Path);
        if (result.ObservedBytes is not null)
            stdout.WriteLine("bytes=" + result.ObservedBytes.Value.ToString(CultureInfo.InvariantCulture));
        if (result.ObservedSha256 is not null)
            stdout.WriteLine("sha256=" + result.ObservedSha256);
        if (result.Message is not null)
            stderr.WriteLine(result.Message);

        return result.Status switch
        {
            ArtifactVerificationStatus.Match => 0,
            ArtifactVerificationStatus.InvalidExpectation => 2,
            _ => 1,
        };
    }
}
