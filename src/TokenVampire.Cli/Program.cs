using TokenVampire.Reporting;

if (args.Length == 0 || (args[0] != "report" && args[0] != "ingest-usage"))
{
    Console.Error.WriteLine(ReportCommand.Usage);
    Console.Error.WriteLine(UsageIngest.Usage);
    return 2;
}

if (args[0] == "ingest-usage")
{
    return UsageIngest.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, Console.Error);
}

using var stdout = Console.OpenStandardOutput();
return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);
