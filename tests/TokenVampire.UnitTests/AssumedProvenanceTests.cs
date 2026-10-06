using TokenVampire.Billing;
using TokenVampire.Domain;
using Xunit;

public class AssumedProvenanceTests
{
    static UsageLine L(Measured<decimal> cost) =>
        new("1", "output", Measured<decimal>.Unknown, Measured<decimal>.Unknown, cost);

    [Fact]
    public void Assumed_is_not_measured()
    {
        Assert.False(Measured<decimal>.Assume(1m).IsMeasured);
        Assert.True(Measured<decimal>.Of(1m).IsMeasured);
        Assert.False(Measured<decimal>.Unknown.IsMeasured);
    }

    [Fact]
    public void Assumed_line_cost_gives_NotEstablished()
    {
        var r = UsageReconciler.Reconcile([L(Measured<decimal>.Assume(1m))], Measured<decimal>.Of(1m));
        Assert.Equal(ReconStatus.NotEstablished, r.Status);
        Assert.Null(r.Difference);
    }

    [Fact]
    public void Assumed_invoice_gives_NotEstablished()
    {
        var r = UsageReconciler.Reconcile([L(Measured<decimal>.Of(1m))], Measured<decimal>.Assume(1m));
        Assert.Equal(ReconStatus.NotEstablished, r.Status);
    }

    [Fact]
    public void Assumed_values_never_produce_Mismatch()
    {
        var r = UsageReconciler.Reconcile([L(Measured<decimal>.Assume(1m))], Measured<decimal>.Assume(9m));
        Assert.Equal(ReconStatus.NotEstablished, r.Status);
    }
}
