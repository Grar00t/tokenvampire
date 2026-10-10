using System.Globalization;
using System.Text;
using TokenVampire.Domain;
using TokenVampire.I18n;

namespace TokenVampire.Reporting;

public static class CaseReport
{
    public static string Render(CaseLedger ledger, string currency) =>
        Render(ledger, currency, Catalog.Builtin(), "en");

    public static string Render(CaseLedger ledger, string currency, Catalog catalog, string locale)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(catalog);
        var cur = Money.Of(0m, currency).Currency;
        var rtl = Locale.DirectionOf(locale) == TextDirection.Rtl;
        Func<string, string> iso = s => rtl ? UnicodeText.Isolate(s) : s;
        var items = catalog.Get("report.items", locale);
        var none = catalog.Get("report.none", locale);
        var sb = new StringBuilder();
        sb.Append(catalog.Get("report.title", locale)).Append(" (").Append(iso(cur)).Append(")\n");
        Line(sb, ledger, cur, catalog.Get("report.funded", locale), ChargeKind.CreditTopUp, none, items, iso);
        Line(sb, ledger, cur, catalog.Get("report.usage", locale), ChargeKind.UsageConsumption, catalog.Get("report.not_measured", locale), items, iso);
        Line(sb, ledger, cur, catalog.Get("report.subs", locale), ChargeKind.Subscription, none, items, iso);
        Line(sb, ledger, cur, catalog.Get("report.seats", locale), ChargeKind.SeatLicense, none, items, iso);
        Line(sb, ledger, cur, catalog.Get("report.refunds", locale), ChargeKind.Refund, none, items, iso);
        Line(sb, ledger, cur, catalog.Get("report.credits", locale), ChargeKind.Credit, none, items, iso);
        sb.Append(catalog.Get("report.gaps", locale)).Append(": ")
          .Append(iso(ledger.Gaps.Count.ToString(CultureInfo.InvariantCulture))).Append('\n');
        foreach (var g in ledger.Gaps.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            sb.Append("- ").Append(iso(g.Key)).Append(": ")
              .Append(BidiSanitizer.Strip(UnicodeText.Nfc(g.Value))).Append('\n');
        }
        sb.Append(catalog.Get("report.boundary", locale)).Append('\n');
        return sb.ToString();
    }

    private static void Line(StringBuilder sb, CaseLedger l, string cur, string label, ChargeKind kind,
        string whenNone, string itemsWord, Func<string, string> iso)
    {
        var sel = l.Items.Where(c => c.Kind == kind && c.Amount.Currency == cur).ToList();
        sb.Append(label).Append(": ");
        if (sel.Count == 0)
        {
            sb.Append(whenNone).Append('\n');
            return;
        }
        var total = sel.Sum(c => c.Amount.Amount);
        // At least two decimal places for compatibility; never round away micro-unit charges.
        sb.Append(iso(total.ToString("0.00##########################", CultureInfo.InvariantCulture)))
          .Append(" (").Append(iso(sel.Count.ToString(CultureInfo.InvariantCulture)))
          .Append(' ').Append(itemsWord).Append(")\n");
    }
}
