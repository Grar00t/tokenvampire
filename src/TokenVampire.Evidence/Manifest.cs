using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TokenVampire.Evidence;

public sealed record Manifest(IReadOnlyList<SealedItem> Items, string RootHash)
{
    public static Manifest Build(IEnumerable<SealedItem> items)
    {
        var list = items.OrderBy(i => i.Id, StringComparer.Ordinal).ToArray();
        if (list.Select(i => i.Id).Distinct().Count() != list.Length)
            throw new InvalidOperationException("Duplicate evidence id.");
        return new(list, Root(list));
    }

    public static string Root(IReadOnlyList<SealedItem> list)
    {
        var sb = new StringBuilder();
        foreach (var i in list) sb.Append(i.Id).Append('\n').Append(i.Sha256).Append('\n');
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    public bool IsIntact() => Root(Items) == RootHash;

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    public static Manifest FromJson(string json) =>
        JsonSerializer.Deserialize<Manifest>(json) ?? throw new InvalidOperationException("Bad manifest.");
}
