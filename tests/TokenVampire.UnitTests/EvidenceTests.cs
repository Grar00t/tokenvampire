using TokenVampire.Evidence;
using Xunit;

public class EvidenceTests
{
    static string Tmp(string text) { var p = Path.GetTempFileName(); File.WriteAllText(p, text); return p; }

    [Fact] public async Task Seal_then_verify_ok()
    {
        var p = Tmp("abc");
        var s = await EvidenceSealer.SealAsync("e1", p, TimeProvider.System, TestContext.Current.CancellationToken);
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", s.Sha256);
        Assert.True(await EvidenceSealer.VerifyAsync(s, p, TestContext.Current.CancellationToken));
    }

    [Fact] public async Task Modified_file_fails_verify()
    {
        var p = Tmp("abc");
        var s = await EvidenceSealer.SealAsync("e1", p, TimeProvider.System, TestContext.Current.CancellationToken);
        File.WriteAllText(p, "abd");
        Assert.False(await EvidenceSealer.VerifyAsync(s, p, TestContext.Current.CancellationToken));
    }

    [Fact] public async Task Manifest_detects_tamper()
    {
        var p = Tmp("abc");
        var s = await EvidenceSealer.SealAsync("e1", p, TimeProvider.System, TestContext.Current.CancellationToken);
        var m = Manifest.Build([s]);
        Assert.True(m.IsIntact());
        var bad = m with { Items = [s with { Sha256 = new string('0', 64) }] };
        Assert.False(bad.IsIntact());
    }

    [Fact] public async Task Manifest_json_roundtrip()
    {
        var p = Tmp("abc");
        var m = Manifest.Build([await EvidenceSealer.SealAsync("e1", p, TimeProvider.System, TestContext.Current.CancellationToken)]);
        Assert.True(Manifest.FromJson(m.ToJson()).IsIntact());
    }

    [Fact] public async Task Duplicate_ids_rejected()
    {
        var p = Tmp("abc");
        var s = await EvidenceSealer.SealAsync("e1", p, TimeProvider.System, TestContext.Current.CancellationToken);
        Assert.Throws<InvalidOperationException>(() => Manifest.Build([s, s]));
    }
}
