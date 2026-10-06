namespace TokenVampire.Domain;

public enum EvidenceState
{
    Unknown = 0,
    Verified,
    ProviderReported,
    UserAsserted,
    Estimated,
    ScenarioOnly
}

public enum ChargeState
{
    Unknown = 0,
    Authorized,
    AuthorizationUnclear,
    UnauthorizedSuspected,
    DuplicateSuspected,
    RenewalUnwanted,
    PriceChanged,
    BillingMismatch
}

public enum OutcomeState
{
    Unknown = 0,
    Delivered,
    PartiallyDelivered,
    NotDelivered,
    Rejected,
    Failed,
    Cancelled
}

public enum DamageLedgerKind
{
    DirectCharge = 1,
    AttributableRework,
    ServiceValueGap,
    ConsequentialHarm
}
