using System.Security.Cryptography;

namespace TokenVampire.Infrastructure;

public enum ArtifactVerificationStatus
{
    Match,
    Missing,
    LinkRejected,
    SizeMismatch,
    HashMismatch,
    Unreadable,
    InvalidExpectation,
}

public sealed record ArtifactVerificationResult(
    ArtifactVerificationStatus Status,
    string Path,
    long? ObservedBytes = null,
    string? ObservedSha256 = null,
    string? Message = null)
{
    public bool Matched => Status == ArtifactVerificationStatus.Match;
}

public static class ArtifactVerifier
{
    public static ArtifactVerificationResult Verify(
        string path,
        string? expectedSha256 = null,
        long? expectedBytes = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Invalid(path ?? string.Empty, "path is required");

        if (expectedBytes is < 0)
            return Invalid(path, "expected byte length must be non-negative");

        string? expectedHash = null;
        if (expectedSha256 is not null)
        {
            var candidate = expectedSha256.Trim();
            if (candidate.Length != 64 || !IsHex(candidate))
                return Invalid(path, "expected SHA-256 must be exactly 64 hexadecimal characters");

            expectedHash = candidate.ToLowerInvariant();
        }

        try
        {
            var info = new FileInfo(path);
            if (!info.Exists)
                return new(ArtifactVerificationStatus.Missing, path, Message: "artifact does not exist");

            if (info.LinkTarget is not null || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                return new(ArtifactVerificationStatus.LinkRejected, path, Message: "symbolic links and reparse points are not accepted as direct artifact evidence");

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var observedBytes = stream.Length;
            var observedHash = Convert.ToHexStringLower(SHA256.HashData(stream));

            if (expectedBytes is not null && observedBytes != expectedBytes.Value)
                return new(ArtifactVerificationStatus.SizeMismatch, path, observedBytes, observedHash,
                    $"expected {expectedBytes.Value} bytes but observed {observedBytes}");

            if (expectedHash is not null && !string.Equals(observedHash, expectedHash, StringComparison.Ordinal))
                return new(ArtifactVerificationStatus.HashMismatch, path, observedBytes, observedHash,
                    $"expected SHA-256 {expectedHash} but observed {observedHash}");

            return new(ArtifactVerificationStatus.Match, path, observedBytes, observedHash);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(ArtifactVerificationStatus.Unreadable, path, Message: e.Message);
        }
    }

    static ArtifactVerificationResult Invalid(string path, string message) =>
        new(ArtifactVerificationStatus.InvalidExpectation, path, Message: message);

    static bool IsHex(string value)
    {
        foreach (var c in value)
        {
            if (c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')
                continue;
            return false;
        }
        return true;
    }
}
