using TokenVampire.Domain;
using Xunit;

public class DomainTests
{
    [Fact] public void Unknown_is_not_zero() { var m = Measured<decimal>.Unknown; Assert.False(m.IsKnown); Assert.Null(m.Value); }
    [Fact] public void Measured_is_known() => Assert.True(Measured<decimal>.Of(0m).IsKnown);
    [Fact] public void Money_currency_mismatch_throws() =>
        Assert.Throws<InvalidOperationException>(() => Money.Of(1, "USD").Add(Money.Of(1, "SAR")));
    [Fact] public void Money_bad_currency_throws() => Assert.Throws<ArgumentException>(() => Money.Of(1, "US"));
    [Fact] public void TopUp_is_not_consumption() =>
        Assert.False(new Charge("c1", ChargeKind.CreditTopUp, Money.Of(11.5m, "USD"), new(2026, 9, 7), "e1").IsConsumption);
    [Fact] public void Verdict_without_evidence_throws() =>
        Assert.Throws<InvalidOperationException>(() => Finding.Create("f1", "x", Verdict.Supported, []));
    [Fact] public void NotEstablished_allowed_without_evidence() =>
        Assert.Equal(Verdict.NotEstablished, Finding.Create("f1", "x", Verdict.NotEstablished, []).Verdict);
}
