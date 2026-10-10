using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TokenVampire.I18n;

namespace TokenVampire.Reporting;

public enum DossierPublication { Unknown = 0, RedactedSummary, RestrictedEvidence }
public enum DossierSourceAccess { Unknown = 0, ReporterSuppliedSummary, PrivateArchiveNotBundled, PublicRedactedArtifact }
public enum DossierProvenance { Unknown = 0, UserAsserted, SourceObserved, Verified }
public enum DossierIncidentRole { Unknown = 0, ReportedFailure, PositiveCounterexample }
public enum DossierFindingCategory { Unknown = 0, Telemetry, Dependency, OAuth, Runtime, CausalHypothesis }

public sealed record DossierSource(string Id, string Description, DossierSourceAccess Access, string? Sha256);
public sealed record DossierIncident(string Id, DateOnly Date, string Product, string FailureClass,
    DossierIncidentRole Role, DossierProvenance Provenance, string[] SourceIds);
public sealed record DossierFinding(string Id, DossierFindingCategory Category, string Statement,
    DossierProvenance Provenance, string[] SourceIds, string VerificationNeeded);
public sealed record DossierDemand(string Id, string Request, string[] IncidentIds);
public sealed record DossierResolution(string Id, string Request);

public sealed record AuditDossier(
    int SchemaVersion, string CaseId, string Vendor, string Product,
    DateOnly PeriodStart, DateOnly PeriodEnd, DossierPublication Publication,
    DossierSource[] Sources, DossierIncident[] Incidents, DossierFinding[] Findings,
    DossierDemand[] Demands, DossierResolution[] RequestedResolutions);

public static class AuditDossierCodec
{
    public const int MaxBytes = 256 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 16,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static bool TryRead(ReadOnlyMemory<byte> utf8, out AuditDossier? dossier, out string error)
    {
        dossier = null;
        error = "Invalid redacted case dossier.";
        if (utf8.Length is 0 or > MaxBytes)
        {
            error = "Case dossier is empty or exceeds the 256 KiB limit.";
            return false;
        }

        try
        {
            using var json = JsonDocument.Parse(utf8, new JsonDocumentOptions { MaxDepth = 16 });
            if (HasDuplicateProperties(json.RootElement))
            {
                error = "Duplicate JSON property names are not permitted.";
                return false;
            }

            var parsed = json.RootElement.Deserialize<AuditDossier>(JsonOptions);
            if (!Validate(parsed, out error)) return false;
            dossier = parsed;
            return true;
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or NotSupportedException)
        {
            error = "Invalid case JSON or unsupported field.";
            return false;
        }
    }

    private static bool HasDuplicateProperties(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
                if (!names.Add(property.Name) || HasDuplicateProperties(property.Value)) return true;
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
                if (HasDuplicateProperties(item)) return true;
        }
        return false;
    }

    private static bool Validate(AuditDossier? dossier, out string error)
    {
        error = "Invalid dossier identity, shape, provenance, or references.";
        if (dossier is null || dossier.SchemaVersion != 1 ||
            dossier.Publication != DossierPublication.RedactedSummary ||
            !Identifier(dossier.CaseId) || !PublicText(dossier.Vendor) || !PublicText(dossier.Product) ||
            dossier.PeriodStart == default || dossier.PeriodEnd < dossier.PeriodStart ||
            dossier.Sources is null or { Length: 0 or > 32 } ||
            dossier.Incidents is null or { Length: 0 or > 128 } ||
            dossier.Findings is null or { Length: > 128 } ||
            dossier.Demands is null or { Length: > 64 } ||
            dossier.RequestedResolutions is null or { Length: > 16 })
            return false;

        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in dossier.Sources)
        {
            if (source is null || !Identifier(source.Id) || !sourceIds.Add(source.Id) ||
                !PublicText(source.Description) || source.Access is DossierSourceAccess.Unknown ||
                (source.Sha256 is not null && !Sha256(source.Sha256)) ||
                (source.Access == DossierSourceAccess.PublicRedactedArtifact && source.Sha256 is null))
                return false;
        }

        var incidentIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var incident in dossier.Incidents)
        {
            if (incident is null || !Identifier(incident.Id) || !incidentIds.Add(incident.Id) ||
                incident.Date < dossier.PeriodStart || incident.Date > dossier.PeriodEnd ||
                !PublicText(incident.Product) || !PublicText(incident.FailureClass) ||
                incident.Role is DossierIncidentRole.Unknown ||
                !References(incident.SourceIds, sourceIds) ||
                !PublicationSafe(incident.Provenance))
                return false;
        }

        var findingIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var finding in dossier.Findings)
        {
            if (finding is null || !Identifier(finding.Id) || !findingIds.Add(finding.Id) ||
                finding.Category is DossierFindingCategory.Unknown ||
                !PublicText(finding.Statement, 1000) ||
                !PublicText(finding.VerificationNeeded, 1000) ||
                !References(finding.SourceIds, sourceIds) ||
                !PublicationSafe(finding.Provenance))
                return false;
        }

        var demandIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var demand in dossier.Demands)
            if (demand is null || !Identifier(demand.Id) || !demandIds.Add(demand.Id) ||
                !PublicText(demand.Request, 1000) || !References(demand.IncidentIds, incidentIds))
                return false;

        var resolutionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var resolution in dossier.RequestedResolutions)
            if (resolution is null || !Identifier(resolution.Id) || !resolutionIds.Add(resolution.Id) ||
                !PublicText(resolution.Request, 1000))
                return false;

        error = string.Empty;
        return true;
    }

    // JSON alone cannot prove source inspection or independent confirmation.
    // A later verified-evidence workflow must independently inspect the sealed bytes.
    private static bool PublicationSafe(DossierProvenance provenance) =>
        provenance is DossierProvenance.Unknown or DossierProvenance.UserAsserted;

    private static bool References(string[]? references, HashSet<string> available) =>
        references is { Length: > 0 and <= 32 } &&
        references.All(value => Identifier(value) && available.Contains(value)) &&
        references.Distinct(StringComparer.Ordinal).Count() == references.Length;

    private static bool Identifier(string? value) =>
        value is { Length: > 0 and <= 100 } &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    private static bool PublicText(string? value, int maxLength = 500) =>
        value is not null && !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength &&
        !value.Any(c => char.IsControl(c) || c is '\u202a' or '\u202b' or '\u202d' or '\u202e' or '\u202c');

    private static bool Sha256(string value) =>
        value.Length == 64 && value.All(Uri.IsHexDigit);

    public static string RenderRedactedSummary(AuditDossier dossier, ReadOnlySpan<byte> originalUtf8)
    {
        ArgumentNullException.ThrowIfNull(dossier);
        if (dossier.Publication != DossierPublication.RedactedSummary)
            throw new InvalidOperationException("Only redacted summaries may be rendered.");

        static string Safe(string value) => BidiSanitizer.Strip(UnicodeText.Nfc(value));
        var sb = new StringBuilder();
        sb.AppendLine("TOKENVAMPIRE REDACTED CASE DOSSIER");
        sb.Append("Case: ").AppendLine(Safe(dossier.CaseId));
        sb.Append("Vendor / product: ").Append(Safe(dossier.Vendor)).Append(" / ").AppendLine(Safe(dossier.Product));
        sb.Append("Period: ").Append(dossier.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
          .Append(" to ").AppendLine(dossier.PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.Append("Input SHA-256: ").AppendLine(Convert.ToHexStringLower(SHA256.HashData(originalUtf8)));
        sb.AppendLine("Evidence boundary: REPORTER-SUPPLIED SUMMARY; NOT independently verified.");
        sb.AppendLine("Private source artifacts, credentials and user identifiers: NOT INCLUDED.");
        sb.AppendLine("INCIDENTS");
        foreach (var incident in dossier.Incidents)
        {
            sb.Append("- ").Append(incident.Id).Append(' ')
              .Append(incident.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
              .Append(" [").Append(incident.Role).Append(" / ").Append(incident.Provenance).Append("] ")
              .AppendLine(Safe(incident.FailureClass));
        }
        sb.AppendLine("FINDINGS");
        foreach (var finding in dossier.Findings)
            sb.Append("- ").Append(finding.Id).Append(" [").Append(finding.Provenance).Append("] ")
              .AppendLine(Safe(finding.Statement));
        sb.AppendLine("REMEDIATION REQUESTS");
        foreach (var demand in dossier.Demands)
            sb.Append("- ").Append(demand.Id).Append(": ").AppendLine(Safe(demand.Request));
        sb.AppendLine("USER-REQUESTED RESOLUTIONS (not established entitlements)");
        foreach (var resolution in dossier.RequestedResolutions)
            sb.Append("- ").Append(resolution.Id).Append(": ").AppendLine(Safe(resolution.Request));
        sb.AppendLine("Verdict: NOT ESTABLISHED by the supplied summary alone.");
        return sb.ToString();
    }
}
