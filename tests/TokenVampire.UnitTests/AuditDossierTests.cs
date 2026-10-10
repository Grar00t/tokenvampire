using System.Text;
using System.Text.Json.Nodes;
using TokenVampire.Reporting;
using Xunit;

public sealed class AuditDossierTests
{
    private static byte[] NotionCase()
    {
        using var resource = typeof(AuditDossierTests).Assembly.GetManifestResourceStream("TokenVampire.Notion202608")
            ?? throw new InvalidOperationException("Redacted test dossier resource missing.");
        using var memory = new MemoryStream();
        resource.CopyTo(memory);
        return memory.ToArray();
    }

    private static JsonObject Mutate() =>
        JsonNode.Parse(Encoding.UTF8.GetString(NotionCase()))!.AsObject();

    private static bool IsAccepted(JsonNode json) =>
        AuditDossierCodec.TryRead(Encoding.UTF8.GetBytes(json.ToJsonString()), out _, out _);

    [Fact]
    public void Notion_case_keeps_four_incidents_and_positive_counterexample()
    {
        Assert.True(AuditDossierCodec.TryRead(NotionCase(), out var dossier, out var error), error);
        Assert.NotNull(dossier);
        Assert.Equal("notion-desktop-2026-08", dossier.CaseId);
        Assert.Equal(DossierPublication.RedactedSummary, dossier.Publication);
        Assert.Equal(4, dossier.Incidents.Length);
        Assert.Equal(7, dossier.Demands.Length);
        Assert.Equal(2, dossier.RequestedResolutions.Length);
        Assert.Equal(9, dossier.Findings.Length);
        Assert.Equal(DossierIncidentRole.PositiveCounterexample, Assert.Single(
            dossier.Incidents, i => i.Id == "N-INC-04").Role);
        Assert.All(dossier.Incidents, i => Assert.Equal(DossierProvenance.UserAsserted, i.Provenance));
    }

    [Fact]
    public void OAuth_credential_exposure_and_common_causality_are_unknown()
    {
        Assert.True(AuditDossierCodec.TryRead(NotionCase(), out var dossier, out _));
        Assert.Equal(DossierProvenance.Unknown, Assert.Single(dossier!.Findings,
            f => f.Id == "N-OAUTH-03").Provenance);
        Assert.Equal(DossierProvenance.Unknown, Assert.Single(dossier.Findings,
            f => f.Id == "N-HYP-01").Provenance);
        Assert.DoesNotContain(dossier.Findings, f => f.Provenance == DossierProvenance.Verified);
    }

    [Theory]
    [InlineData("verified")]
    [InlineData("sourceObserved")]
    public void Uninspected_records_cannot_claim_verified_evidence(string provenance)
    {
        var root = Mutate();
        root["findings"]!.AsArray()[0]!.AsObject()["provenance"] = provenance;
        Assert.False(IsAccepted(root));
    }

    [Fact]
    public void Missing_or_unknown_source_references_fail_closed()
    {
        var root = Mutate();
        var sourceIds = new JsonArray();
        sourceIds.Add("NOT-AN-EVIDENCE-SOURCE");
        root["incidents"]!.AsArray()[0]!.AsObject()["sourceIds"] = sourceIds;
        Assert.False(IsAccepted(root));

        root = Mutate();
        root["sources"] = new JsonArray();
        Assert.False(IsAccepted(root));
    }

    [Fact]
    public void Duplicate_json_keys_and_unexpected_secret_fields_are_rejected()
    {
        var duplicated = Encoding.UTF8.GetString(NotionCase()).Replace(
            "\"schemaVersion\": 1,", "\"schemaVersion\": 1, \"schemaVersion\": 1,", StringComparison.Ordinal);
        Assert.False(AuditDossierCodec.TryRead(Encoding.UTF8.GetBytes(duplicated), out _, out _));
        var root = Mutate();
        root["rawOAuthBlob"] = "unredacted-sensitive-data";
        Assert.False(IsAccepted(root));
    }

    [Fact]
    public void Input_is_bounded_and_enums_cannot_be_numbers()
    {
        Assert.False(AuditDossierCodec.TryRead(new byte[AuditDossierCodec.MaxBytes + 1], out _, out _));
        var root = Mutate();
        root["incidents"]!.AsArray()[0]!.AsObject()["provenance"] = 3;
        Assert.False(IsAccepted(root));
    }

    [Fact]
    public void Local_cli_outputs_redacted_case_without_promoting_claims()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var rc = AuditDossierCommand.Run(
            ["--input", "notion.json"], _ => NotionCase(), output, error);
        Assert.Equal(0, rc);
        Assert.Equal(string.Empty, error.ToString());
        var text = output.ToString();
        Assert.Contains("N-INC-04", text);
        Assert.Contains("PositiveCounterexample", text);
        Assert.Contains("N-OAUTH-03 [Unknown]", text);
        Assert.Contains("Verdict: NOT ESTABLISHED", text);
        Assert.Contains("Input SHA-256:", text);
        Assert.DoesNotContain("IndependentlyVerified", text);
    }

    [Fact]
    public void Cli_rejects_bad_arguments_and_malformed_bytes()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        Assert.Equal(2, AuditDossierCommand.Run([], _ => NotionCase(), output, error));
        Assert.Equal(2, AuditDossierCommand.Run(
            ["--input", "bad.json"], _ => Encoding.UTF8.GetBytes("{"), output, error));
        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public void Private_archive_is_referenced_but_never_bundled()
    {
        Assert.True(AuditDossierCodec.TryRead(NotionCase(), out var dossier, out _));
        Assert.Contains(dossier!.Sources, s => s.Access == DossierSourceAccess.PrivateArchiveNotBundled && s.Sha256 is null);
        var bytes = Encoding.UTF8.GetString(NotionCase());
        Assert.DoesNotContain("microsoft_oauth_token_value", bytes);
        Assert.DoesNotContain("PRIVATE KEY-----", bytes);
        Assert.DoesNotContain("Notion.exe file bytes", bytes);
    }
}
