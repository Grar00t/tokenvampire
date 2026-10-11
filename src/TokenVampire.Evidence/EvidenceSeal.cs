using System.Security.Cryptography;

namespace TokenVampire.Evidence;

public sealed record SealedItem(string Id, string FileName, long Length, string Sha256, DateTimeOffset SealedAt)
{
    // Stable logical directory identifier. Never serialize a user's absolute filesystem path.
    public string DirectoryId { get; init; } = "unspecified";
}

public static class EvidenceSealer
{
    public static async Task<SealedItem> SealAsync(string id, string path, TimeProvider clock, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(clock);
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        var hash = await SHA256.HashDataAsync(fs, ct);
        return new SealedItem(id, Path.GetFileName(path), fs.Length, Convert.ToHexStringLower(hash), clock.GetUtcNow());
    }

    public static async Task<bool> VerifyAsync(SealedItem item, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        try
        {
            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            if (fs.Length != item.Length || !string.Equals(Path.GetFileName(path), item.FileName, StringComparison.Ordinal))
                return false;
            if (item.Sha256 is null || item.Sha256.Length != 64 || !item.Sha256.All(Uri.IsHexDigit))
                return false;
            var hash = await SHA256.HashDataAsync(fs, ct);
            return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(item.Sha256), hash);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }
}
