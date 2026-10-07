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

    [Fact]
    public void Direct_symbolic_link_is_rejected()
    {
        var dir = Path.Combine(Path.GetTempPath(), "tokenvampire-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var target = Path.Combine(dir, "target.bin");
        var link = Path.Combine(dir, "link.bin");
        File.WriteAllText(target, "abc");
        try
        {
            File.CreateSymbolicLink(link, target);
            var r = ArtifactVerifier.Verify(link);
            Assert.Equal(ArtifactVerificationStatus.LinkRejected, r.Status);
        }
        catch (UnauthorizedAccessException) when (OperatingSystem.IsWindows())
        {
            return;
        }
        finally
        {
            if (File.Exists(link) || new FileInfo(link).LinkTarget is not null)
                File.Delete(link);
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Parent_symbolic_link_is_rejected()
    {
        var root = Path.Combine(Path.GetTempPath(), "tokenvampire-" + Guid.NewGuid().ToString("N"));
        var targetDir = Path.Combine(root, "target");
        var linkDir = Path.Combine(root, "link");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "artifact.bin"), "abc");
        try
        {
            Directory.CreateSymbolicLink(linkDir, targetDir);
            var r = ArtifactVerifier.Verify(Path.Combine(linkDir, "artifact.bin"));
            Assert.Equal(ArtifactVerificationStatus.LinkRejected, r.Status);
        }
        catch (UnauthorizedAccessException) when (OperatingSystem.IsWindows())
        {
            return;
        }
        finally
        {
            if (Directory.Exists(linkDir) || new DirectoryInfo(linkDir).LinkTarget is not null)
                Directory.Delete(linkDir);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Unix_character_device_is_rejected_before_hashing()
    {
        if (OperatingSystem.IsWindows() || !File.Exists("/dev/zero"))
            return;

        var r = ArtifactVerifier.Verify("/dev/zero");
        Assert.Equal(ArtifactVerificationStatus.Unreadable, r.Status);
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
