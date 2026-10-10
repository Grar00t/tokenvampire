using TokenVampire.Reporting;

if (args.Length == 0)
{
    Console.Error.WriteLine(ReportCommand.Usage);
    Console.Error.WriteLine(AuditDossierCommand.Usage);
    return 2;
}

if (args[0] == "audit-case")
    return AuditDossierCommand.Run(args[1..], ReadBoundedCase, Console.Out, Console.Error);

if (args[0] == "report")
{
    using var stdout = Console.OpenStandardOutput();
    return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);
}

Console.Error.WriteLine(ReportCommand.Usage);
Console.Error.WriteLine(AuditDossierCommand.Usage);
return 2;

static byte[] ReadBoundedCase(string path)
{
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    if (stream.Length > AuditDossierCodec.MaxBytes)
        throw new InvalidDataException("Case dossier size limit exceeded.");
    var buffer = new byte[checked((int)stream.Length)];
    stream.ReadExactly(buffer);
    return buffer;
}
