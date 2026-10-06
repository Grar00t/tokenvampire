using Microsoft.Data.Sqlite;
using TokenVampire.Evidence;

namespace TokenVampire.Infrastructure;

public sealed class EvidenceStore : IDisposable
{
    readonly SqliteConnection _db;

    public EvidenceStore(string path)
    {
        _db = new SqliteConnection($"Data Source={path}");
        _db.Open();
        Exec("PRAGMA journal_mode=WAL;");
        Exec("""
            CREATE TABLE IF NOT EXISTS evidence(
              id TEXT PRIMARY KEY, file_name TEXT NOT NULL, length INTEGER NOT NULL,
              sha256 TEXT NOT NULL, sealed_at TEXT NOT NULL);
            """);
    }

    public void Add(SealedItem i)
    {
        using var c = _db.CreateCommand();
        c.CommandText = "INSERT INTO evidence VALUES($id,$f,$l,$h,$t)";
        c.Parameters.AddWithValue("$id", i.Id);
        c.Parameters.AddWithValue("$f", i.FileName);
        c.Parameters.AddWithValue("$l", i.Length);
        c.Parameters.AddWithValue("$h", i.Sha256);
        c.Parameters.AddWithValue("$t", i.SealedAt.ToString("O"));
        c.ExecuteNonQuery();
    }

    public IReadOnlyList<SealedItem> All()
    {
        using var c = _db.CreateCommand();
        c.CommandText = "SELECT id,file_name,length,sha256,sealed_at FROM evidence ORDER BY id";
        using var r = c.ExecuteReader();
        var list = new List<SealedItem>();
        while (r.Read())
            list.Add(new(r.GetString(0), r.GetString(1), r.GetInt64(2), r.GetString(3), DateTimeOffset.Parse(r.GetString(4))));
        return list;
    }

    void Exec(string sql) { using var c = _db.CreateCommand(); c.CommandText = sql; c.ExecuteNonQuery(); }
    public void Dispose() => _db.Dispose();
}
