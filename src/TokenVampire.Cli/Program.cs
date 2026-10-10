using TokenVampire.Billing;
using TokenVampire.Reporting;

if (args.Length == 0)
{
    Console.Error.WriteLine(ReportCommand.Usage);
    Console.Error.WriteLine(ReasoningAuditCommand.Usage);
    return 2;
}

if (args[0] == "reasoning-audit")
    return ReasoningAuditCommand.Run(args[1..], ReadBoundedInput, Console.Out, Console.Error);

if (args[0] != "report")
{
    Console.Error.WriteLine(ReportCommand.Usage);
    Console.Error.WriteLine(ReasoningAuditCommand.Usage);
    return 2;
}

using var stdout = Console.OpenStandardOutput();
return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);

static byte[] ReadBoundedInput(string path)
{
    using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    if (input.Length > ReasoningAuditCommand.MaxBytes)
        throw new IOException("Reasoning audit input exceeds the size limit.");
    var bytes = new byte[checked((int)input.Length)];
    input.ReadExactly(bytes);
    return bytes;
}
