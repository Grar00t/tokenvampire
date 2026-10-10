using System.Text;
using System.Text.Json.Nodes;
using TokenVampire.Reporting;
using Xunit;

public sealed class OAuthSecurityDossierTests
{
    private static byte[] Fixture()
    {
        using var stream = typeof(OAuthSecurityDossierTests).Assembly.GetManifestResourceStream(
            "TokenVampire.UnattributedOAuth20261010")
            ?? throw new InvalidOperationException("Redacted OAuth dossier test fixture missing.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static JsonObject Json() => JsonNode.Parse(Fixture())!.AsObject();

    private static bool Accepts(JsonObject json) =>
        AuditDossierCodec.TryRead(Encoding.UTF8.GetBytes(json.ToJsonString()), out _, out _);

    [Fact]
    public void Security_dossier_is_redacted_and_not_attributed_to_a_vendor()
    {
        Assert.True(AuditDossierCodec.TryRead(Fixture(), out var dossier, out var error), error);
        Assert.NotNull(dossier);
        Assert.Equal("oauth-metadata-telemetry-2026-10-10", dossier.CaseId);
        Assert.Equal("Unattributed", dossier.Vendor);
        Assert.Equal(DossierPublication.RedactedSummary, dossier.Publication);
        Assert.Equal(2, dossier.Incidents.Length);
        Assert.Equal(9, dossier.Findings.Length);
        Assert.Equal(4, dossier.Demands.Length);
        Assert.Empty(dossier.RequestedResolutions);
        Assert.All(dossier.Sources, source =>
        {
            Assert.Equal(DossierSourceAccess.ReporterSuppliedSummary, source.Access);
            Assert.Null(source.Sha256);
        });
    }

    [Fact]
    public void Active_admin_and_exfiltration_conclusions_remain_unknown()
    {
        Assert.True(AuditDossierCodec.TryRead(Fixture(), out var dossier, out _));
        Assert.NotNull(dossier);
        Assert.Equal(DossierProvenance.UserAsserted, Assert.Single(
            dossier.Findings, x => x.Id == "U-OAUTH-02").Provenance);
        Assert.Equal(DossierProvenance.Unknown, Assert.Single(
            dossier.Findings, x => x.Id == "U-OAUTH-03").Provenance);
        Assert.Equal(DossierProvenance.Unknown, Assert.Single(
            dossier.Findings, x => x.Id == "U-TEL-03").Provenance);
        Assert.Equal(DossierProvenance.Unknown, Assert.Single(
            dossier.Findings, x => x.Id == "U-RAG-02").Provenance);
        Assert.DoesNotContain(dossier.Findings, x =>
            x.Provenance is DossierProvenance.Verified or DossierProvenance.SourceObserved);
        Assert.Contains(dossier.Findings, x => x.Category == DossierFindingCategory.Security);
        Assert.Contains(dossier.Findings, x => x.Category == DossierFindingCategory.Retrieval);
    }

    [Theory]
    [InlineData("verified")]
    [InlineData("sourceObserved")]
    public void Self_report_cannot_promote_sensitive_claims_to_verified(string provenance)
    {
        var json = Json();
        json["findings"]!.AsArray()[0]!.AsObject()["provenance"] = provenance;
        Assert.False(Accepts(json));
    }

    [Fact]
    public void Missing_source_reference_and_unexpected_raw_credentials_fail_closed()
    {
        var json = Json();
        json["incidents"]!.AsArray()[0]!.AsObject()["sourceIds"] =
            new JsonArray("NOT-AN-INSPECTED-ARTIFACT");
        Assert.False(Accepts(json));

        json = Json();
        json["accessToken"] = "redaction-test-only";
        Assert.False(Accepts(json));
    }

    [Fact]
    public void Public_case_rejects_synthetic_credential_shaped_values()
    {
        var json = Json();
        json["findings"]!.AsArray()[0]!.AsObject()["statement"] =
            "synthetic credential " + string.Concat("ya", "29.") + new string('Q', 80);
        Assert.False(Accepts(json));

        json = Json();
        json["sources"]!.AsArray()[0]!.AsObject()["description"] =
            "synthetic Authorization: Bearer test-value";
        Assert.False(Accepts(json));
    }

    [Fact]
    public void Missing_measurements_are_not_recast_as_zero_or_evidence_of_exfiltration()
    {
        Assert.True(AuditDossierCodec.TryRead(Fixture(), out var dossier, out _));
        Assert.NotNull(dossier);
        var count = Assert.Single(dossier.Findings, x => x.Id == "U-TEL-01");
        Assert.Equal(DossierProvenance.UserAsserted, count.Provenance);
        Assert.Contains("87000", count.Statement);
        var exfil = Assert.Single(dossier.Findings, x => x.Id == "U-TEL-03");
        Assert.Equal(DossierProvenance.Unknown, exfil.Provenance);
    }

    [Fact]
    public void Cli_outputs_redacted_summary_and_no_false_security_verdict()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exit = AuditDossierCommand.Run(
            ["--input", "redacted.json"], _ => Fixture(), output, error);
        Assert.Equal(0, exit);
        Assert.Equal(string.Empty, error.ToString());
        var text = output.ToString();
        Assert.Contains("U-OAUTH-03 [Unknown]", text);
        Assert.Contains("U-TEL-03 [Unknown]", text);
        Assert.Contains("U-RAG-02 [Unknown]", text);
        Assert.Contains("Input SHA-256:", text);
        Assert.Contains("Verdict: NOT ESTABLISHED", text);
        Assert.DoesNotContain("administrative access verified", text, StringComparison.OrdinalIgnoreCase);
    }
}
