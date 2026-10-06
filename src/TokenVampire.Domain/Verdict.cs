namespace TokenVampire.Domain;

public enum Verdict { NotEstablished, Supported, Contradicted }

public sealed record Finding(string Id, string Claim, Verdict Verdict, IReadOnlyList<string> EvidenceIds)
{
    public static Finding Create(string id, string claim, Verdict v, IEnumerable<string> evidence)
    {
        var ids = evidence.Distinct().ToArray();
        if (v != Verdict.NotEstablished && ids.Length == 0)
            throw new InvalidOperationException("A verdict other than NotEstablished needs evidence.");
        return new(id, claim, v, ids);
    }
}
