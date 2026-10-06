using TokenVampire.Billing;
using TokenVampire.Domain;
using Xunit;

public class BillingTests
{
    static Charge C(string id, ChargeKind k, decimal a) => new(id, k, Money.Of(a, "USD"), new(2026, 9, 7), "e");
    static UsageLine L(string id, decimal? cost) => new(id, "output", Measured<decimal>.Unknown, Measured<decimal>.Unknown,
        cost is null ? Measured<decimal>.Unknown : Measured<decimal>.Of(cost.Value));

    [Fact] public void TopUp_not_counted_as_consumption()
    {
        var s = Ledger.Summarize([C("a", ChargeKind.CreditTopUp, 11.5m)]);
        Assert.Equal(11.5m, s.TopUps["USD"]);
        Assert.Empty(s.Consumption);
    }

    [Fact] public void Duplicate_charge_counted_once()
    {
        var s = Ledger.Summarize([C("a", ChargeKind.CreditTopUp, 10m), C("a", ChargeKind.CreditTopUp, 10m)]);
        Assert.Equal(10m, s.TopUps["USD"]);
        Assert.Equal(["a"], s.DuplicateIds);
    }

    [Fact] public void Currencies_not_mixed()
    {
        var s = Ledger.Summarize([C("a", ChargeKind.UsageConsumption, 1m),
            new Charge("b", ChargeKind.UsageConsumption, Money.Of(2m, "SAR"), new(2026, 9, 7), "e")]);
        Assert.Equal(1m, s.Consumption["USD"]);
        Assert.Equal(2m, s.Consumption["SAR"]);
    }

    [Fact] public void Unknown_line_gives_NotEstablished()
    {
        var r = UsageReconciler.Reconcile([L("1", 1m), L("2", null)], Measured<decimal>.Of(1m));
        Assert.Equal(ReconStatus.NotEstablished, r.Status);
    }

    [Fact] public void Unknown_invoice_gives_NotEstablished() =>
        Assert.Equal(ReconStatus.NotEstablished, UsageReconciler.Reconcile([L("1", 1m)], Measured<decimal>.Unknown).Status);

    [Fact] public void Matching_total_is_Matched() =>
        Assert.Equal(ReconStatus.Matched, UsageReconciler.Reconcile([L("1", 1m), L("2", 2m)], Measured<decimal>.Of(3m)).Status);

    [Fact] public void Different_total_is_Mismatch()
    {
        var r = UsageReconciler.Reconcile([L("1", 1m)], Measured<decimal>.Of(3m));
        Assert.Equal(ReconStatus.Mismatch, r.Status);
        Assert.Equal(2m, r.Difference);
    }
}
