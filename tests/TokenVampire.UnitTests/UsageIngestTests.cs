using System.Text;
using TokenVampire.Reporting;
using Xunit;

public class UsageIngestTests
{
    private const string Ledger = """
        {"items":[{"id":"p01","kind":"CreditTopUp","amount":11.5,"currency":"USD","date":"2026-09-07","evidence":"e1"}],
         "gaps":{"g1":"No billed amount"}}
        """;

    private const string Head = "date,model,requests,input_tokens,output_tokens,cached_input\n";

    private static (int Code, string Err, Dictionary<string, byte[]> Files) Run(string csv, string ledger, params string[] args)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var se = new StringWriter();
        var code = UsageIngest.Run(
            args,
            p => p switch
            {
                "u.csv" => Encoding.UTF8.GetBytes(csv),
                "l.json" => Encoding.UTF8.GetBytes(ledger),
                _ => throw new FileNotFoundException(p),
            },
            (p, b) => files[p] = b,
            se);
        return (code, se.ToString(), files);
    }

    private static string[] Args(string from = "2026-09-06", string to = "2026-09-09") =>
        ["--export", "u.csv", "--ledger", "l.json", "--from", from, "--to", to];

    [Fact]
    public void Missing_days_stay_unknown_and_are_never_zero_rows()
    {
        var csv = Head + "2026-09-07,m1,10,100,50,0\n2026-09-09,m2,6,20,5,0\n";
        var r = Run(csv, Ledger, Args());
        Assert.Equal(0, r.Code);
        var text = Encoding.UTF8.GetString(r.Files["l.json"]);
        Assert.Contains("2 days with data, requests=16, input_tokens=120, output_tokens=55, cached_input=0, models=m1,m2", text, StringComparison.Ordinal);
        Assert.Contains("Days with no data are unknown, not zero: 2026-09-06, 2026-09-08", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Existing_items_and_gaps_are_preserved_and_result_parses()
    {
        var csv = Head + "2026-09-07,m1,1,1,1,0\n";
        var r = Run(csv, Ledger, Args());
        Assert.Equal(0, r.Code);
        var bytes = r.Files["l.json"];
        Assert.True(LedgerJson.TryParse(bytes, out var ledger, out _));
        Assert.Equal(1, ledger.Count);
        Assert.Contains("g1", ledger.Gaps.Keys);
        Assert.Contains(UsageIngest.SummaryGap, ledger.Gaps.Keys);
        Assert.Contains(UsageIngest.MissingGap, ledger.Gaps.Keys);
        Assert.Contains(UsageIngest.AccountGap, ledger.Gaps.Keys);
    }

    [Fact]
    public void Account_attribution_is_always_declared_unverified()
    {
        var csv = Head + "2026-09-06,m1,1,1,1,0\n2026-09-07,m1,1,1,1,0\n2026-09-08,m1,1,1,1,0\n2026-09-09,m1,1,1,1,0\n";
        var r = Run(csv, Ledger, Args());
        Assert.Equal(0, r.Code);
        var text = Encoding.UTF8.GetString(r.Files["l.json"]);
        Assert.Contains("attribution to the dispute not verified", text, StringComparison.Ordinal);
        Assert.DoesNotContain(UsageIngest.MissingGap, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Header_only_export_marks_every_day_unknown()
    {
        var r = Run(Head, Ledger, Args());
        Assert.Equal(0, r.Code);
        var text = Encoding.UTF8.GetString(r.Files["l.json"]);
        Assert.Contains("0 days with data", text, StringComparison.Ordinal);
        Assert.Contains("2026-09-06, 2026-09-07, 2026-09-08, 2026-09-09", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Duplicate_date_model_is_rejected_and_nothing_is_written()
    {
        var csv = Head + "2026-09-07,m1,1,1,1,0\n2026-09-07,m1,2,2,2,0\n";
        var r = Run(csv, Ledger, Args());
        Assert.Equal(2, r.Code);
        Assert.Contains("duplicate", r.Err, StringComparison.Ordinal);
        Assert.Empty(r.Files);
    }

    [Fact]
    public void Bad_input_is_rejected_without_writing()
    {
        Assert.Equal(2, Run(Head + "2026-09-10,m1,1,1,1,0\n", Ledger, Args()).Code);
        Assert.Equal(2, Run(Head + "2026-09-07,m1,-1,1,1,0\n", Ledger, Args()).Code);
        Assert.Equal(2, Run(Head + "2026-09-07,m1,1,1,1\n", Ledger, Args()).Code);
        Assert.Equal(2, Run(Head + "2026-09-07,\"m1\",1,1,1,0\n", Ledger, Args()).Code);
        Assert.Equal(2, Run("date,model\n", Ledger, Args()).Code);
        Assert.Equal(2, Run(Head, Ledger, Args("2026-09-09", "2026-09-06")).Code);
        var r = Run(Head + "2026-09-07,m1,1.5,1,1,0\n", Ledger, Args());
        Assert.Equal(2, r.Code);
        Assert.Empty(r.Files);
    }

    [Fact]
    public void Existing_usage_gap_id_is_rejected()
    {
        var first = Run(Head + "2026-09-07,m1,1,1,1,0\n", Ledger, Args());
        var again = Run(Head + "2026-09-07,m1,1,1,1,0\n", Encoding.UTF8.GetString(first.Files["l.json"]), Args());
        Assert.Equal(2, again.Code);
        Assert.Contains("already exists", again.Err, StringComparison.Ordinal);
        Assert.Empty(again.Files);
    }

    [Fact]
    public void Out_option_leaves_ledger_path_untouched()
    {
        var r = Run(Head + "2026-09-07,m1,1,1,1,0\n", Ledger, [.. Args(), "--out", "o.json"]);
        Assert.Equal(0, r.Code);
        Assert.True(r.Files.ContainsKey("o.json"));
        Assert.False(r.Files.ContainsKey("l.json"));
    }

    [Fact]
    public void Missing_required_option_and_unreadable_file_exit_2()
    {
        Assert.Equal(2, Run(Head, Ledger, "--export", "u.csv").Code);
        Assert.Equal(2, Run(Head, Ledger, "--bogus", "x").Code);
        Assert.Equal(2, Run(Head, Ledger, "--export", "nope.csv", "--ledger", "l.json", "--from", "2026-09-06", "--to", "2026-09-09").Code);
    }
}
