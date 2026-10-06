using System.Globalization;
using System.Text;
using TokenVampire.Domain;

namespace TokenVampire.Reporting;

public static class CaseReport
{
    public static string Render(CaseLedger ledger, string currency)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        var cur = Money.Of(0m, currency).Currency;
        var sb = new StringBuilder();
        sb.Append("CASE REPORT (").Append(cur).Append(")\n");
        Line(sb, ledger, cur, "Funded (credit top-ups, not spend)", ChargeKind.CreditTopUp, "none recorded");
        Line(sb, ledger, cur, "Billed usage (consumption)", ChargeKind.UsageConsumption, "NOT MEASURED");
        Line(sb, ledger, cur, "Subscriptions", ChargeKind.Subscription, "none recorded");
        Line(sb, ledger, cur, "Seat licenses", ChargeKind.SeatLicense, "none recorded");
        Line(sb, ledger, cur, "Refunds", ChargeKind.Refund, "none recorded");
        Line(sb, ledger, cur, "Credits", ChargeKind.Credit, "none recorded");
        sb.Append("Declared gaps: ").Append(ledger.Gaps.Count.ToString(CultureInfo.InvariantCulture)).Append('\n');
        foreach (var g in ledger.Gaps.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            sb.Append("- ").Append(g.Key).Append(": ").Append(g.Value).Append('\n');
        }
        sb.Append("Boundary: a missing figure is unknown, not zero. This is not a legal refund entitlement.\n");
        return sb.ToString();
    }

    private static void Line(StringBuilder sb, CaseLedger l, string cur, string label, ChargeKind kind, string whenNone)
    {
        var sel = l.Items.Where(c => c.Kind == kind && c.Amount.Currency == cur).ToList();
        sb.Append(label).Append(": ");
        if (sel.Count == 0)
        {
            sb.Append(whenNone).Append('\n');
            return;
        }
        var total = sel.Sum(c => c.Amount.Amount);
        sb.Append(total.ToString("0.00", CultureInfo.InvariantCulture))
          .Append(" (").Append(sel.Count.ToString(CultureInfo.InvariantCulture)).Append(" item(s))\n");
    }
}
