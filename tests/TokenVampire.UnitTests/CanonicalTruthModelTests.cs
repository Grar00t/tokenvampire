using TokenVampire.Domain;
using Xunit;

public class CanonicalTruthModelTests
{
    [Fact]
    public void Canonical_state_defaults_are_unknown()
    {
        Assert.Equal(EvidenceState.Unknown, default(EvidenceState));
        Assert.Equal(ChargeState.Unknown, default(ChargeState));
        Assert.Equal(OutcomeState.Unknown, default(OutcomeState));
    }

    [Fact]
    public void Damage_kind_has_no_silent_default_category() =>
        Assert.False(Enum.IsDefined(default(DamageLedgerKind)));

    [Fact]
    public void Measured_damage_requires_evidence() =>
        Assert.Throws<InvalidOperationException>(() =>
            DamageEntry.Create(
                "d1",
                DamageLedgerKind.DirectCharge,
                Measured<decimal>.Of(10m),
                "USD",
                []));

    [Fact]
    public void Empty_damage_ledger_has_no_verified_total()
    {
        var total = new DamageLedger().VerifiedTotal("USD");
        Assert.False(total.IsMeasured);
        Assert.Null(total.Value);
    }

    [Fact]
    public void Unknown_amount_prevents_verified_total()
    {
        var ledger = new DamageLedger();
        ledger.Add(DamageEntry.Create(
            "d1",
            DamageLedgerKind.DirectCharge,
            Measured<decimal>.Unknown,
            "USD",
            []));

        Assert.False(ledger.VerifiedTotal("USD").IsMeasured);
    }

    [Fact]
    public void Assumed_amount_prevents_verified_total()
    {
        var ledger = new DamageLedger();
        ledger.Add(DamageEntry.Create(
            "d1",
            DamageLedgerKind.AttributableRework,
            Measured<decimal>.Assume(25m),
            "USD",
            []));

        Assert.False(ledger.VerifiedTotal("USD").IsMeasured);
    }

    [Fact]
    public void All_measured_evidenced_amounts_produce_verified_total()
    {
        var ledger = new DamageLedger();
        ledger.Add(DamageEntry.Create(
            "d1",
            DamageLedgerKind.DirectCharge,
            Measured<decimal>.Of(10m),
            "USD",
            ["e1"]));
        ledger.Add(DamageEntry.Create(
            "d2",
            DamageLedgerKind.AttributableRework,
            Measured<decimal>.Of(2.5m),
            "usd",
            ["e2"]));

        var total = ledger.VerifiedTotal("USD");

        Assert.True(total.IsMeasured);
        Assert.Equal(Money.Of(12.5m, "USD"), total.Value!.Value);
    }

    [Fact]
    public void No_evidence_still_cannot_create_supported_finding() =>
        Assert.Throws<InvalidOperationException>(() =>
            Finding.Create("f1", "claim", Verdict.Supported, []));
}
