using System.Text;
using TokenVampire.Billing;
using TokenVampire.Domain;
using TokenVampire.Evidence;
using TokenVampire.Parsing;
using TokenVampire.Reporting;
using Xunit;

public sealed class AuditHardeningTests
{
    private static SealedItem Item() =>
        new("e1", "case.txt", 3, new string('a', 64), new DateTimeOffset(2026, 10, 10, 0, 0, 0, TimeSpan.Zero))
        { DirectoryId = "case-vault" };

    [Theory]
    [InlineData("name")]
    [InlineData("length")]
    [InlineData("time")]
    [InlineData("directory")]
    [InlineData("hash")]
    public void Manifest_commits_to_every_metadata_field(string field)
    {
        var s = Item();
        var original = Manifest.Build([s]);
        var changed = field switch
        {
            "name" => s with { FileName = "other.txt" },
            "length" => s with { Length = 99 },
            "time" => s with { SealedAt = s.SealedAt.AddSeconds(1) },
            "directory" => s with { DirectoryId = "another" },
            _ => s with { Sha256 = new string('b', 64) }
        };
        Assert.False((original with { Items = [changed] }).IsIntact());
    }

    [Fact]
    public void Manifest_rejects_reordering_and_invalid_hash()
    {
        var a = Item();
        var b = a with { Id = "e2" };
        var manifest = Manifest.Build([b, a]);
        Assert.True(manifest.IsIntact());
        Assert.False((manifest with { Items = [b, a] }).IsIntact());
        Assert.False((manifest with { Items = [a with { Sha256 = "x" }, b] }).IsIntact());
    }

    [Fact]
    public void Manifest_json_round_trip_preserves_directory_id()
    {
        var copy = Manifest.FromJson(Manifest.Build([Item()]).ToJson());
        Assert.True(copy.IsIntact());
        Assert.Equal("case-vault", Assert.Single(copy.Items).DirectoryId);
    }

    [Fact]
    public void Unverified_observation_cannot_be_supported()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AuditObservation.Create("a", AuditControl.DeliveryTruth, "delivered", AuditResult.Supported,
                EvidenceValue.Estimated, ["e1"]));
        Assert.Throws<InvalidOperationException>(() =>
            AuditObservation.Create("a", AuditControl.DeliveryTruth, "delivered", AuditResult.Supported,
                EvidenceValue.Verified, []));
    }

    [Fact]
    public void Verified_observation_requires_reference()
    {
        var found = AuditObservation.Create("a", AuditControl.DeliveryTruth, "matched SHA-256",
            AuditResult.Supported, EvidenceValue.Verified, ["e1", "e1"], id => id == "e1");
        Assert.Single(found.EvidenceIds);
    }

    [Fact]
    public void Evidence_identifier_without_inspection_cannot_verify_claim()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AuditObservation.Create("a", AuditControl.DeliveryTruth, "delivered",
                AuditResult.Supported, EvidenceValue.Verified, ["e1"]));
        Assert.Throws<InvalidOperationException>(() =>
            AuditObservation.Create("a", AuditControl.DeliveryTruth, "delivered",
                AuditResult.Supported, EvidenceValue.Verified, ["e1"], _ => false));
    }

    [Fact]
    public void Revoked_consent_blocks_export()
    {
        var now = DateTimeOffset.UtcNow;
        var receipt = new ConsentReceipt(ConsentPurpose.ExportToRecipient, "reviewer", now.AddHours(-2),
            now.AddHours(-1), "e1");
        Assert.False(ConsentGate.MayExport(receipt, "reviewer", now));
        Assert.False(ConsentGate.MayExport(null, "reviewer", now));
    }

    [Fact]
    public void Token_estimation_is_assumed_not_measured()
    {
        var data = Encoding.UTF8.GetBytes("""{"messages":[{"content":"hello world"}]}""");
        var result = LocalTokenEstimator.FromJson(data);
        Assert.True(result.Tokens.IsKnown);
        Assert.False(result.Tokens.IsMeasured);
        Assert.Equal(Provenance.Assumed, result.Tokens.Provenance);
        Assert.False(result.RemainingEstimate.IsKnown);
    }

    [Fact]
    public void Invalid_json_and_missing_content_remain_unknown()
    {
        Assert.False(LocalTokenEstimator.FromJson(Encoding.UTF8.GetBytes("{")).Tokens.IsKnown);
        Assert.False(LocalTokenEstimator.FromJson(Encoding.UTF8.GetBytes("""{"cost":0}""")).Tokens.IsKnown);
    }

    [Fact]
    public void Missing_costs_block_rework_total()
    {
        var attempts = new[] { new Attempt("a", "q", true, Measured<decimal>.Unknown,
            Measured<decimal>.Unknown) };
        var result = AuditMetrics.Calculate(attempts, true);
        Assert.False(result.TokenBurn.IsMeasured);
        Assert.False(result.ReworkCost.IsMeasured);
        Assert.False(result.ZeroInferenceOccurrences.IsMeasured);
    }

    [Fact]
    public void Measured_micro_charges_are_not_rounded_to_zero()
    {
        var ledger = new CaseLedger();
        ledger.Add(new Charge("a", ChargeKind.UsageConsumption, Money.Of(0.00000019m, "USD"),
            new DateOnly(2026, 10, 10), "e1"));
        Assert.Contains("0.00000019", CaseReport.Render(ledger, "USD"));
    }

    [Fact]
    public void Subscription_classification_is_not_token_consumption()
    {
        var charge = new Charge("a", ChargeKind.Subscription, Money.Of(200m, "USD"),
            new DateOnly(2026, 10, 10), "e1")
        { Classification = ChargeClassification.ProSubscriptionUnmetered };
        Assert.False(charge.IsConsumption);
    }
}
