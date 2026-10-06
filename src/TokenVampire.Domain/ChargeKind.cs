namespace TokenVampire.Domain;

public enum ChargeKind { CreditTopUp, Subscription, SeatLicense, UsageConsumption, Refund, Credit }

public sealed record Charge(string Id, ChargeKind Kind, Money Amount, DateOnly Date, string EvidenceId)
{
    public bool IsConsumption => Kind == ChargeKind.UsageConsumption;
}
