using System.Globalization;
using Microsoft.Data.Sqlite;
using TokenVampire.Domain;

namespace TokenVampire.Infrastructure;

public sealed class CaseStore : IDisposable
{
    private readonly SqliteConnection _db;

    public CaseStore(string path)
    {
        _db = new SqliteConnection("Data Source=" + path);
        _db.Open();
        using var c = _db.CreateCommand();
        c.CommandText = "CREATE TABLE IF NOT EXISTS charges(id TEXT PRIMARY KEY, kind INTEGER NOT NULL, amount TEXT NOT NULL, currency TEXT NOT NULL, date TEXT NOT NULL, evidence_id TEXT NOT NULL)";
        c.ExecuteNonQuery();
    }

    public void Add(Charge ch)
    {
        ArgumentNullException.ThrowIfNull(ch);
        using var c = _db.CreateCommand();
        c.CommandText = "INSERT INTO charges(id,kind,amount,currency,date,evidence_id) VALUES($i,$k,$a,$c,$d,$e)";
        c.Parameters.AddWithValue("$i", ch.Id);
        c.Parameters.AddWithValue("$k", (int)ch.Kind);
        c.Parameters.AddWithValue("$a", ch.Amount.Amount.ToString(CultureInfo.InvariantCulture));
        c.Parameters.AddWithValue("$c", ch.Amount.Currency);
        c.Parameters.AddWithValue("$d", ch.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        c.Parameters.AddWithValue("$e", ch.EvidenceId);
        c.ExecuteNonQuery();
    }

    public CaseLedger Load()
    {
        var ledger = new CaseLedger();
        using var c = _db.CreateCommand();
        c.CommandText = "SELECT id,kind,amount,currency,date,evidence_id FROM charges ORDER BY date,id";
        using var r = c.ExecuteReader();
        while (r.Read())
        {
            ledger.Add(new Charge(
                r.GetString(0),
                (ChargeKind)r.GetInt32(1),
                Money.Of(decimal.Parse(r.GetString(2), CultureInfo.InvariantCulture), r.GetString(3)),
                DateOnly.ParseExact(r.GetString(4), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                r.GetString(5)));
        }
        return ledger;
    }

    public void Dispose() => _db.Dispose();
}
