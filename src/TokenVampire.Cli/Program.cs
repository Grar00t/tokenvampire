using System.Globalization;
using TokenVampire.Infrastructure;
using TokenVampire.Reporting;

if (args.Length == 0)
{
    WriteUsage();
    return 2;
}

if (args[0] == "report")
{
    using var stdout = Console.OpenStandardOutput();
    return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);
}

if (args[0] == "verify-artifact")
    return RunVerifyArtifact(args[1..]);

WriteUsage();
return 2;

static int RunVerifyArtifact(string[] commandArgs)
{
    string? path = null;
    string? expectedSha256 = null;
    long? expectedBytes = null;

    for (var i = 0; i < commandArgs.Length; i += 2)
    {
        if (i + 1 >= commandArgs.Length)
        {
            Console.Error.WriteLine("missing value for " + commandArgs[i]);
            WriteUsage();
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
                    Console.Error.WriteLine("--bytes must be a non-negative integer");
                    return 2;
                }
                expectedBytes = parsed;
                break;
            default:
                Console.Error.WriteLine("unknown option " + commandArgs[i]);
                WriteUsage();
                return 2;
        }
    }

    if (string.IsNullOrWhiteSpace(path))
    {
        Console.Error.WriteLine("--path is required");
        WriteUsage();
        return 2;
    }

    var result = ArtifactVerifier.Verify(path, expectedSha256, expectedBytes);
    Console.Out.WriteLine("status=" + result.Status.ToString().ToUpperInvariant());
    Console.Out.WriteLine("path=" + result.Path);
    if (result.ObservedBytes is not null)
        Console.Out.WriteLine("bytes=" + result.ObservedBytes.Value.ToString(CultureInfo.InvariantCulture));
    if (result.ObservedSha256 is not null)
        Console.Out.WriteLine("sha256=" + result.ObservedSha256);
    if (result.Message is not null)
        Console.Error.WriteLine(result.Message);

    return result.Status switch
    {
        ArtifactVerificationStatus.Match => 0,
        ArtifactVerificationStatus.InvalidExpectation => 2,
        _ => 1,
    };
}

static void WriteUsage()
{
    Console.Error.WriteLine(ReportCommand.Usage);
    Console.Error.WriteLine("usage: verify-artifact --path <file> [--sha256 <64-hex>] [--bytes <non-negative-int>]");
}
