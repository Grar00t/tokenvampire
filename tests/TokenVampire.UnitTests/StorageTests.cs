using System.Security.Cryptography;
using TokenVampire.Evidence;
using TokenVampire.Infrastructure;
using Xunit;

public class StorageTests
{
    static Vault V(out string dir, byte[]? key = null)
    {
        dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        return new Vault(dir, key ?? RandomNumberGenerator.GetBytes(32));
    }

    [Fact]
    public async Task Vault_roundtrip()
    {
        var v = V(out _);
        await v.PutAsync("e1", [1, 2, 3], TestContext.Current.CancellationToken);
        Assert.Equal(new byte[] { 1, 2, 3 }, await v.GetAsync("e1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Vault_file_is_not_plaintext()
    {
        var v = V(out var dir);
        var plain = System.Text.Encoding.UTF8.GetBytes("SECRET-MARKER-12345");
        await v.PutAsync("e1", plain, TestContext.Current.CancellationToken);
        var raw = await File.ReadAllBytesAsync(Path.Combine(dir, "e1.bin"), TestContext.Current.CancellationToken);
        Assert.DoesNotContain("SECRET-MARKER", System.Text.Encoding.UTF8.GetString(raw));
    }

    [Fact]
    public async Task Vault_tamper_detected()
    {
        var v = V(out var dir);
        await v.PutAsync("e1", [9, 9, 9, 9], TestContext.Current.CancellationToken);
        var p = Path.Combine(dir, "e1.bin");
        var raw = await File.ReadAllBytesAsync(p, TestContext.Current.CancellationToken);
        raw[^1] ^= 0xFF;
        await File.WriteAllBytesAsync(p, raw, TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<CryptographicException>(() => v.GetAsync("e1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Vault_wrong_key_fails()
    {
        var v1 = V(out var dir);
        await v1.PutAsync("e1", [1], TestContext.Current.CancellationToken);
        var v2 = new Vault(dir, RandomNumberGenerator.GetBytes(32));
        await Assert.ThrowsAnyAsync<CryptographicException>(() => v2.GetAsync("e1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Vault_rejects_path_traversal() =>
        await Assert.ThrowsAsync<ArgumentException>(() => V(out _).PutAsync("../x", [1], TestContext.Current.CancellationToken));

    [Fact]
    public void Store_roundtrip()
    {
        var db = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
        using var s = new EvidenceStore(db);
        s.Add(new SealedItem("e1", "a.txt", 3, new string('a', 64), DateTimeOffset.UtcNow));
        Assert.Single(s.All());
        Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() =>
            s.Add(new SealedItem("e1", "a.txt", 3, new string('a', 64), DateTimeOffset.UtcNow)));
    }
}
