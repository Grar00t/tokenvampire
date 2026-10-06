using Microsoft.Data.Sqlite;
using TokenVampire.Domain;
using TokenVampire.Infrastructure;
using Xunit;

public class CaseStoreTests
{
    private static string Tmp() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");

    private static Charge C(string id, ChargeKind k, decimal a) =>
        new(id, k, Money.Of(a, "USD"), new(2026, 9, 7), "e-" + id);

    [Fact]
    public void Ledger_survives_reopen_and_keeps_kinds()
    {
        var db = Tmp();
        using (var s = new CaseStore(db))
        {
            s.Add(C("p01", ChargeKind.CreditTopUp, 11.5m));
            s.Add(C("p02", ChargeKind.UsageConsumption, 0.25m));
        }
        using var s2 = new CaseStore(db);
        var l = s2.Load();
        Assert.Equal(2, l.Count);
        Assert.Equal(11.5m, l.Funded("USD").Amount);
        Assert.Equal(0.25m, l.Consumption("USD").Amount);
    }

    [Fact]
    public void Duplicate_id_throws()
    {
        using var s = new CaseStore(Tmp());
        s.Add(C("p01", ChargeKind.CreditTopUp, 1m));
        Assert.Throws<SqliteException>(() => s.Add(C("p01", ChargeKind.CreditTopUp, 1m)));
    }
}
