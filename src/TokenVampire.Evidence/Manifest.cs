using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TokenVampire.Evidence;

public sealed record Manifest(IReadOnlyList<SealedItem> Items, string RootHash)
{
    // v2 binds all metadata. Legacy manifests need explicit re-sealing; silent migration is unsafe.
    public static Manifest Build(IEnumerable<SealedItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var list = items.OrderBy(i => i.Id, StringComparer.Ordinal).ToArray();
        Validate(list);
        return new(list, Root(list));
    }

    public static string Root(IReadOnlyList<SealedItem> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        Validate(list);
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("TokenVampire.Manifest.v2");
            writer.Write(list.Count);
            foreach (var item in list)
            {
                Write(writer, item.Id);
                Write(writer, item.FileName);
                writer.Write(item.Length);
                Write(writer, item.Sha256.ToLowerInvariant());
                Write(writer, item.SealedAt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                Write(writer, item.DirectoryId);
            }
        }
        return Convert.ToHexStringLower(SHA256.HashData(stream.ToArray()));
    }

    private static void Write(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static void Validate(IReadOnlyList<SealedItem> items)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        string? prior = null;
        foreach (var item in items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Id) ||
                string.IsNullOrWhiteSpace(item.FileName) || string.IsNullOrWhiteSpace(item.DirectoryId) ||
                item.Length < 0 || item.Sha256 is null || item.Sha256.Length != 64 ||
                !item.Sha256.All(Uri.IsHexDigit))
                throw new InvalidOperationException("Invalid manifest evidence metadata.");
            if (!seen.Add(item.Id)) throw new InvalidOperationException("Duplicate evidence id.");
            if (prior is not null && StringComparer.Ordinal.Compare(prior, item.Id) > 0)
                throw new InvalidOperationException("Manifest items must be sorted.");
            prior = item.Id;
        }
    }

    public bool IsIntact()
    {
        try
        {
            if (RootHash is null || RootHash.Length != 64 || !RootHash.All(Uri.IsHexDigit))
                return false;
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(Root(Items)), Convert.FromHexString(RootHash));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NullReferenceException)
        {
            return false;
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    public static Manifest FromJson(string json) =>
        JsonSerializer.Deserialize<Manifest>(json) ?? throw new InvalidOperationException("Bad manifest.");
}
