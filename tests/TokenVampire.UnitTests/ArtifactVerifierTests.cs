using System.Text;
using TokenVampire.Infrastructure;
using Xunit;

public class ArtifactVerifierTests
{
    const string AbcSha256 = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    [Fact]
    public void Matching_hash_and_length_return_match()
    {
        WithFile("abc", path =>
        {
            var r = ArtifactVerifier.Verify(path, AbcSha256.ToUpperInvariant(), 3);
            Assert.Equal(ArtifactVerificationStatus.Match, r.Status);
            Assert.True(r.Matched);
            Assert.Equal(3, r.ObservedBytes);
            Assert.Equal(AbcSha256, r.ObservedSha256);
        });
    }

    [Fact]
    public void Wrong_hash_returns_hash_mismatch_with_observed_hash()
    {
        WithFile("abc", path =>
        {
            var r = ArtifactVerifier.Verify(path, new string('0', 64), 3);
            Assert.Equal(ArtifactVerificationStatus.HashMismatch, r.Status);
            Assert.Equal(AbcSha256, r.ObservedSha256);
        });
    }

    [Fact]
    public void Wrong_length_returns_size_mismatch()
    {
        WithFile("abc", path =>
        {
            var r = ArtifactVerifier.Verify(path, expectedBytes: 4);
            Assert.Equal(ArtifactVerificationStatus.SizeMismatch, r.Status);
            Assert.Equal(3, r.ObservedBytes);
        });
    }

    [Fact]
    public void Missing_file_is_not_reported_as_match()
    {
        var path = Path.Combine(Path.GetTempPath(), "tokenvampire-" + Guid.NewGuid().ToString("N"), "missing.bin");
        var r = ArtifactVerifier.Verify(path);
        Assert.Equal(ArtifactVerificationStatus.Missing, r.Status);
        Assert.False(r.Matched);
    }

    [Fact]
    public void Invalid_hash_expectation_is_rejected_before_io()
    {
        var r = ArtifactVerifier.Verify("does-not-matter", "xyz");
        Assert.Equal(ArtifactVerificationStatus.InvalidExpectation, r.Status);
    }

    [Fact]
    public void Negative_length_expectation_is_rejected()
    {
        var r = ArtifactVerifier.Verify("does-not-matter", expectedBytes: -1);
        Assert.Equal(ArtifactVerificationStatus.InvalidExpectation, r.Status);
    }

    static void WithFile(string text, Action<string> test)
    {
        var dir = Path.Combine(Path.GetTempPath(), "tokenvampire-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "artifact.bin");
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(text));
        try
        {
            test(path);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
