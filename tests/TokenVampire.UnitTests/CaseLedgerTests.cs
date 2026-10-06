using TokenVampire.Domain;
using Xunit;

public class CaseLedgerTests
{
    private static Charge TopUp(string id, decimal amt, string cur = "USD") =>
        new(id, ChargeKind.CreditTopUp, Money.Of(amt, cur), new(2026, 9, 7), "e-" + id);

    [Fact]
    public void TopUp_counts_as_funded_not_consumption()
    {
        var l = new CaseLedger();
        Assert.True(l.Add(TopUp("p01", 11.5m)));
        Assert.Equal(0m, l.Consumption("USD").Amount);
        Assert.Equal(11.5m, l.Funded("USD").Amount);
    }

    [Fact]
    public void Duplicate_id_is_rejected_and_not_double_counted()
    {
        var l = new CaseLedger();
        Assert.True(l.Add(TopUp("p01", 11.5m)));
        Assert.False(l.Add(TopUp("p01", 11.5m)));
        Assert.Equal(1, l.Count);
        Assert.Equal(11.5m, l.Funded("USD").Amount);
    }

    [Fact]
    public void Currencies_are_never_mixed()
    {
        var l = new CaseLedger();
        l.Add(TopUp("a", 10m, "USD"));
        l.Add(TopUp("b", 40m, "SAR"));
        Assert.Equal(10m, l.Funded("USD").Amount);
        Assert.Equal(40m, l.Funded("SAR").Amount);
    }
}
