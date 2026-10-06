using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TokenVampire.Reporting;

public static class UsageIngest
{
    public const string Usage = "usage: ingest-usage --export <usage.csv> --ledger <file.json> --from <yyyy-MM-dd> --to <yyyy-MM-dd> [--out <file>]";
    public const string SummaryGap = "usage_summary";
    public const string MissingGap = "usage_missing_days";
    public const string AccountGap = "usage_account";
    private const string Header = "date,model,requests,input_tokens,output_tokens,cached_input";
    private const int MaxDays = 366;

    public static int Run(string[] args, Func<string, byte[]> read, Action<string, byte[]> write, TextWriter stderr)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(stderr);
        string? exportPath = null;
        string? ledgerPath = null;
        string? outPath = null;
        string? fromText = null;
        string? toText = null;
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
                case "--export": exportPath = val; break;
                case "--ledger": ledgerPath = val; break;
                case "--out": outPath = val; break;
                case "--from": fromText = val; break;
                case "--to": toText = val; break;
                default:
                    stderr.WriteLine("unknown option " + args[i]);
                    stderr.WriteLine(Usage);
                    return 2;
            }
        }
        if (string.IsNullOrWhiteSpace(exportPath) || string.IsNullOrWhiteSpace(ledgerPath)
            || string.IsNullOrWhiteSpace(fromText) || string.IsNullOrWhiteSpace(toText))
        {
            stderr.WriteLine("--export, --ledger, --from and --to are required");
            stderr.WriteLine(Usage);
            return 2;
        }
        if (!DateOnly.TryParseExact(fromText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from)
            || !DateOnly.TryParseExact(toText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
        {
            stderr.WriteLine("--from and --to must be yyyy-MM-dd");
            return 2;
        }
        if (to < from || to.DayNumber - from.DayNumber + 1 > MaxDays)
        {
            stderr.WriteLine("range must satisfy from <= to and span at most " + MaxDays.ToString(CultureInfo.InvariantCulture) + " days");
            return 2;
        }
        byte[] csvBytes;
        byte[] ledgerBytes;
        try
        {
            csvBytes = read(exportPath);
            ledgerBytes = read(ledgerPath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("cannot read input: " + e.Message);
            return 2;
        }
        string csv;
        try
        {
            csv = new UTF8Encoding(false, true).GetString(csvBytes);
        }
        catch (DecoderFallbackException)
        {
            stderr.WriteLine("export is not valid UTF-8");
            return 2;
        }
        if (!TryBuildGaps(csv, from, to, out var gaps, out var error))
        {
            stderr.WriteLine(error);
            return 2;
        }
        if (!TryMerge(ledgerBytes, gaps, out var merged, out error))
        {
            stderr.WriteLine(error);
            return 2;
        }
        try
        {
            write(outPath ?? ledgerPath, merged);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            stderr.WriteLine("cannot write output: " + e.Message);
            return 2;
        }
        return 0;
    }

    public static bool TryBuildGaps(string csv, DateOnly from, DateOnly to, out List<KeyValuePair<string, string>> gaps, out string error)
    {
        ArgumentNullException.ThrowIfNull(csv);
        gaps = [];
        error = string.Empty;
        var rows = csv.TrimStart('\uFEFF').Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        while (rows.Count > 0 && rows[^1].Length == 0)
        {
            rows.RemoveAt(rows.Count - 1);
        }
        if (rows.Count == 0 || rows[0] != Header)
        {
            error = "export header must be: " + Header;
            return false;
        }
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var days = new HashSet<DateOnly>();
        var models = new SortedSet<string>(StringComparer.Ordinal);
        long requests = 0, input = 0, output = 0, cached = 0;
        try
        {
            for (var n = 1; n < rows.Count; n++)
            {
                var where = "row " + (n + 1).ToString(CultureInfo.InvariantCulture) + ": ";
                var line = rows[n];
                if (line.Contains('"'))
                {
                    error = where + "quoted fields are not supported";
                    return false;
                }
                var p = line.Split(',');
                if (p.Length != 6)
                {
                    error = where + "expected 6 columns";
                    return false;
                }
                if (!DateOnly.TryParseExact(p[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    error = where + "date must be yyyy-MM-dd";
                    return false;
                }
                if (d < from || d > to)
                {
                    error = where + "date " + p[0] + " is outside the requested range";
                    return false;
                }
                var model = p[1];
                if (model.Length == 0 || !model.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or ':' or '-' or '/'))
                {
                    error = where + "invalid model name";
                    return false;
                }
                if (!seen.Add(p[0] + "|" + model))
                {
                    error = where + "duplicate date+model";
                    return false;
                }
                if (!Num(p[2], out var rq) || !Num(p[3], out var inp) || !Num(p[4], out var outp) || !Num(p[5], out var ca))
                {
                    error = where + "counts must be non-negative integers";
                    return false;
                }
                requests = checked(requests + rq);
                input = checked(input + inp);
                output = checked(output + outp);
                cached = checked(cached + ca);
                days.Add(d);
                models.Add(model);
            }
        }
        catch (OverflowException)
        {
            error = "counts overflow";
            return false;
        }
        var nl = CultureInfo.InvariantCulture;
        gaps.Add(new(SummaryGap, "Usage export " + Day(from) + ".." + Day(to) + ": "
            + days.Count.ToString(nl) + " days with data, requests=" + requests.ToString(nl)
            + ", input_tokens=" + input.ToString(nl) + ", output_tokens=" + output.ToString(nl)
            + ", cached_input=" + cached.ToString(nl) + ", models=" + string.Join(',', models)));
        var missing = new List<string>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (!days.Contains(d))
            {
                missing.Add(Day(d));
            }
        }
        if (missing.Count > 0)
        {
            gaps.Add(new(MissingGap, "Days with no data are unknown, not zero: " + string.Join(", ", missing)));
        }
        gaps.Add(new(AccountGap, "Export has no account/organization column: attribution to the dispute not verified"));
        return true;
    }

    private static bool TryMerge(byte[] ledgerBytes, List<KeyValuePair<string, string>> gaps, out byte[] merged, out string error)
    {
        merged = [];
        error = string.Empty;
        var mem = ledgerBytes.AsMemory();
        if (mem.Length >= 3 && ledgerBytes[0] == 0xEF && ledgerBytes[1] == 0xBB && ledgerBytes[2] == 0xBF)
        {
            mem = mem[3..];
        }
        JsonObject? root;
        try
        {
            root = JsonNode.Parse(mem.Span, null, new JsonDocumentOptions { MaxDepth = 8 }) as JsonObject;
        }
        catch (JsonException e)
        {
            error = "invalid ledger: " + e.Message;
            return false;
        }
        if (root is null)
        {
            error = "ledger root must be an object";
            return false;
        }
        JsonObject target;
        if (root["gaps"] is null)
        {
            target = new JsonObject();
            root["gaps"] = target;
        }
        else if (root["gaps"] is JsonObject existing)
        {
            target = existing;
        }
        else
        {
            error = "gaps must be an object";
            return false;
        }
        foreach (var g in gaps)
        {
            if (target.ContainsKey(g.Key))
            {
                error = "gap id already exists: " + g.Key;
                return false;
            }
        }
        foreach (var g in gaps)
        {
            target[g.Key] = g.Value;
        }
        var opts = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        merged = new UTF8Encoding(false).GetBytes(root.ToJsonString(opts));
        if (!LedgerJson.TryParse(merged, out _, out var why))
        {
            merged = [];
            error = "merged ledger is invalid: " + why;
            return false;
        }
        return true;
    }

    private static bool Num(string s, out long v) =>
        long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out v);

    private static string Day(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
