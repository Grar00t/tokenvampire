using System.Security.Cryptography;
using TokenVampire.Evidence;
using TokenVampire.Infrastructure;
using Xunit;

public class IngestTests
{
    sealed record Env(Ingestor Ing, Vault Vault, EvidenceStore Store, string Dir);

    static Env Make(long max = 1 << 20)
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var v = new Vault(Path.Combine(dir, "vault"), RandomNumberGenerator.GetBytes(32));
        var s = new EvidenceStore(Path.Combine(dir, "e.db"));
        return new Env(new Ingestor(v, s, max), v, s, dir);
    }

    [Fact]
    public async Task Arabic_file_name_roundtrip()
    {
        var e = Make();
        var f = Path.Combine(e.Dir, "تقرير_الفاتورة.txt");
        File.WriteAllBytes(f, [1, 2, 3, 4]);
        var item = await e.Ing.IngestAsync(f, TestContext.Current.CancellationToken);
        Assert.Equal("تقرير_الفاتورة.txt", item.FileName);
        Assert.Equal(4, item.Length);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await e.Vault.GetAsync(item.Id, TestContext.Current.CancellationToken));
        Assert.Single(e.Store.All());
    }

    [Fact]
    public void Name_is_normalized_to_nfc() =>
        Assert.Equal("\u00e9.txt", FileNamePolicy.Normalize("e\u0301.txt"));

    [Theory]
    [InlineData("a\u202Etxt.exe")]
    [InlineData("CON.txt")]
    [InlineData("lpt1")]
    [InlineData("a*b.txt")]
    [InlineData("trail.")]
    [InlineData("trail ")]
    [InlineData("x\u0000y.txt")]
    [InlineData("")]
    public void Hostile_names_rejected(string name) =>
        Assert.Throws<IngestException>(() => FileNamePolicy.Normalize(name));

    [Fact]
    public async Task Oversize_rejected()
    {
        var e = Make(10);
        var f = Path.Combine(e.Dir, "big.bin");
        File.WriteAllBytes(f, new byte[11]);
        await Assert.ThrowsAsync<IngestException>(() => e.Ing.IngestAsync(f, TestContext.Current.CancellationToken));
        Assert.Empty(e.Store.All());
    }

    [Fact]
    public async Task Missing_file_rejected()
    {
        var e = Make();
        await Assert.ThrowsAsync<IngestException>(() =>
            e.Ing.IngestAsync(Path.Combine(e.Dir, "nope.txt"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Duplicate_content_stored_once()
    {
        var e = Make();
        var a = Path.Combine(e.Dir, "a.txt");
        var b = Path.Combine(e.Dir, "b.txt");
        File.WriteAllBytes(a, [7, 7]);
        File.WriteAllBytes(b, [7, 7]);
        var i1 = await e.Ing.IngestAsync(a, TestContext.Current.CancellationToken);
        var i2 = await e.Ing.IngestAsync(b, TestContext.Current.CancellationToken);
        Assert.Equal(i1.Id, i2.Id);
        Assert.Single(e.Store.All());
    }

    [Fact]
    public async Task Symlink_rejected()
    {
        var e = Make();
        var target = Path.Combine(e.Dir, "t.txt");
        var link = Path.Combine(e.Dir, "l.txt");
        File.WriteAllBytes(target, [1]);
        try { File.CreateSymbolicLink(link, target); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { return; }
        await Assert.ThrowsAsync<IngestException>(() => e.Ing.IngestAsync(link, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Salt_is_persisted()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var s1 = KeyMaterial.LoadOrCreateSalt(dir);
        var s2 = KeyMaterial.LoadOrCreateSalt(dir);
        Assert.Equal(16, s1.Length);
        Assert.Equal(s1, s2);
    }
}
