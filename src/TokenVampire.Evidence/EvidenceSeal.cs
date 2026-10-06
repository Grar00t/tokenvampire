using System.Security.Cryptography;

namespace TokenVampire.Evidence;

public sealed record SealedItem(string Id, string FileName, long Length, string Sha256, DateTimeOffset SealedAt);

public static class EvidenceSealer
{
    public static async Task<SealedItem> SealAsync(string id, string path, TimeProvider clock, CancellationToken ct = default)
    {
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        var hash = await SHA256.HashDataAsync(fs, ct);
        return new SealedItem(id, Path.GetFileName(path), fs.Length, Convert.ToHexStringLower(hash), clock.GetUtcNow());
    }

    public static async Task<bool> VerifyAsync(SealedItem item, string path, CancellationToken ct = default)
    {
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        if (fs.Length != item.Length) return false;
        var hash = await SHA256.HashDataAsync(fs, ct);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(item.Sha256), hash);
    }
}
