using System.Security.Cryptography;
using TokenVampire.Evidence;

namespace TokenVampire.Infrastructure;

public sealed class Ingestor(Vault vault, EvidenceStore store, long maxBytes = 64L * 1024 * 1024)
{
    public async Task<SealedItem> IngestAsync(string path, CancellationToken ct = default)
    {
        var info = new FileInfo(path);
        if (!info.Exists) throw new IngestException("File not found.");
        if (info.LinkTarget is not null || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IngestException("Links are not accepted.");
        if (info.Length > maxBytes) throw new IngestException("File exceeds size limit.");

        var name = FileNamePolicy.Normalize(info.Name);

        byte[] data;
        await using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            if (fs.Length > maxBytes) throw new IngestException("File exceeds size limit.");
            data = new byte[fs.Length];
            await fs.ReadExactlyAsync(data, ct);
        }

        var hash = Convert.ToHexStringLower(SHA256.HashData(data));
        var existing = store.All().FirstOrDefault(i => i.Id == hash);
        if (existing is not null) return existing;

        await vault.PutAsync(hash, data, ct);
        var item = new SealedItem(hash, name, data.Length, hash, DateTimeOffset.UtcNow);
        store.Add(item);
        return item;
    }
}
