using TokenVampire.Domain;
using TokenVampire.Reporting;
using Xunit;

public class CaseReportTests
{
    private static Charge C(string id, ChargeKind k, decimal a) =>
        new(id, k, Money.Of(a, "USD"), new(2026, 9, 7), "e-" + id);

    [Fact]
    public void No_usage_is_not_measured_not_zero()
    {
        var l = new CaseLedger();
        l.Add(C("p01", ChargeKind.CreditTopUp, 11.5m));
        var r = CaseReport.Render(l, "USD");
        Assert.Contains("Funded (credit top-ups, not spend): 11.50 (1 item(s))", r);
        Assert.Contains("Billed usage (consumption): NOT MEASURED", r);
        Assert.DoesNotContain("consumption): 0.00", r);
    }

    [Fact]
    public void Usage_is_reported_when_recorded()
    {
        var l = new CaseLedger();
        l.Add(C("u1", ChargeKind.UsageConsumption, 0.25m));
        Assert.Contains("Billed usage (consumption): 0.25 (1 item(s))", CaseReport.Render(l, "USD"));
    }

    [Fact]
    public void Gaps_are_listed_and_deduped()
    {
        var l = new CaseLedger();
        Assert.True(l.AddGap("g1", "No complete usage export"));
        Assert.False(l.AddGap("g1", "dup"));
        var r = CaseReport.Render(l, "USD");
        Assert.Contains("Declared gaps: 1", r);
        Assert.Contains("- g1: No complete usage export", r);
    }
}
