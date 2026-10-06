using TokenVampire.Reporting;

if (args.Length == 0 || args[0] != "report")
{
    Console.Error.WriteLine(ReportCommand.Usage);
    return 2;
}

using var stdout = Console.OpenStandardOutput();
return ReportCommand.Run(args[1..], File.ReadAllBytes, File.WriteAllBytes, stdout, Console.Error);
