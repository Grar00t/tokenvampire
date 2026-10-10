namespace TokenVampire.Domain;

public enum EvidenceValue { Unknown = 0, ProviderReported, UserAsserted, Estimated, Verified }
public enum AuditControl { DeliveryTruth = 1, SemanticSupport, CostAndRework, EditorialConsistency, UserAgencyAndPrivacy, ChallengeAndRemedy }
public enum AuditResult { Unknown = 0, Supported, Contradicted }
public enum ChargeClassification { Unknown = 0, ProSubscriptionUnmetered, ZeroInferenceBilling, AutomatedBotSupportNoRemedy, UnmetDelivery }
public enum RemedyStatus { Unknown = 0, Requested, ProviderResponded, Rejected, Approved, Issued }
public enum RiskFunction { Govern, Map, Measure, Manage }
public enum DisclosureKind { ServiceTerms, DeliveryTerms, Pricing, Invoice, RefundTerms }
public enum ConsentPurpose { StoreEvidence, ExportToRecipient, ShareWithReviewer }

public sealed record AuditObservation(
    string Id, AuditControl Control, string Claim, AuditResult Result,
    EvidenceValue EvidenceState, IReadOnlyList<string> EvidenceIds)
{
    public static AuditObservation Create(
        string id, AuditControl control, string claim, AuditResult result,
        EvidenceValue evidenceState, IEnumerable<string> evidenceIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(claim);
        ArgumentNullException.ThrowIfNull(evidenceIds);
        if (!Enum.IsDefined(control) || control == 0) throw new ArgumentOutOfRangeException(nameof(control));
        var ids = evidenceIds.Select(x => string.IsNullOrWhiteSpace(x) ? throw new ArgumentException("Blank evidence id.") : x)
            .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (result != AuditResult.Unknown &&
            (evidenceState != EvidenceValue.Verified || ids.Length == 0))
            throw new InvalidOperationException("A supported/contradicted finding requires verified evidence references.");
        return new(id, control, claim, result, evidenceState, Array.AsReadOnly(ids));
    }
}

public sealed record DisclosureRecord(
    string Id, DisclosureKind Kind, DateTimeOffset ObservedAt, string EvidenceId, EvidenceValue State);
public sealed record GovernanceMapping(
    string Framework, string Reference, RiskFunction? Function, string EvidenceId, EvidenceValue State);
public sealed record SupportEvent(
    string TicketId, DateTimeOffset ObservedAt, bool AutomatedReply, string EvidenceId, RemedyStatus Remedy);
public sealed record DeliveryRecord(
    string ClaimId, string ArtifactSha256, DateTimeOffset ObservedAt, string PayloadStatus, string EvidenceId);
public sealed record CitationMapping(
    string ClaimId, string EvidenceId, string PassageHash, bool? ExactQuoteMatches, bool? SemanticSupportVerified);
public sealed record EditorialEvent(
    string CaseId, DateTimeOffset ObservedAt, string PromptHash, bool? UninstructedDrift,
    bool? ContextTruncated, string EvidenceId);

public sealed record ConsentReceipt(
    ConsentPurpose Purpose, string Recipient, DateTimeOffset GrantedAt, DateTimeOffset? RevokedAt, string EvidenceId)
{
    public bool IsActiveAt(DateTimeOffset now) =>
        !string.IsNullOrWhiteSpace(EvidenceId) && !string.IsNullOrWhiteSpace(Recipient) &&
        GrantedAt <= now && (RevokedAt is null || RevokedAt > now);
}

public static class ConsentGate
{
    public static bool MayExport(ConsentReceipt? receipt, string recipient, DateTimeOffset now) =>
        receipt is not null && receipt.Purpose == ConsentPurpose.ExportToRecipient &&
        string.Equals(receipt.Recipient, recipient, StringComparison.Ordinal) && receipt.IsActiveAt(now);
}
