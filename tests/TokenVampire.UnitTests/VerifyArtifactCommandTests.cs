using System.Text;
using Xunit;

public class VerifyArtifactCommandTests
{
    const string AbcSha256 = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    [Fact]
    public void Match_contract_returns_zero_and_required_fields()
    {
        WithFile("abc", path =>
        {
            var stdout = new StringWriter();
            var stderr = new StringWriter();

            var exit = VerifyArtifactCommand.Run(
                ["--path", path, "--sha256", AbcSha256, "--bytes", "3"],
                stdout,
                stderr);

            Assert.Equal(0, exit);
            Assert.Contains("status=MATCH", stdout.ToString());
            Assert.Contains("path=" + path, stdout.ToString());
            Assert.Contains("bytes=3", stdout.ToString());
            Assert.Contains("sha256=" + AbcSha256, stdout.ToString());
            Assert.Equal(string.Empty, stderr.ToString());
        });
    }

    [Fact]
    public void Mismatch_contract_returns_one()
    {
        WithFile("abc", path =>
        {
            var stdout = new StringWriter();
            var stderr = new StringWriter();

            var exit = VerifyArtifactCommand.Run(
                ["--path", path, "--sha256", new string('0', 64)],
                stdout,
                stderr);

            Assert.Equal(1, exit);
            Assert.Contains("status=HASHMISMATCH", stdout.ToString());
            Assert.NotEqual(string.Empty, stderr.ToString());
        });
    }

    [Fact]
    public void Invalid_expectation_contract_returns_two()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = VerifyArtifactCommand.Run(
            ["--path", "unused", "--sha256", "xyz"],
            stdout,
            stderr);

        Assert.Equal(2, exit);
        Assert.Contains("status=INVALIDEXPECTATION", stdout.ToString());
    }

    [Fact]
    public void Invalid_option_returns_two_without_verifying()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exit = VerifyArtifactCommand.Run(["--wat", "x"], stdout, stderr);

        Assert.Equal(2, exit);
        Assert.Contains("unknown option --wat", stderr.ToString());
        Assert.Contains(VerifyArtifactCommand.Usage, stderr.ToString());
    }

    static void WithFile(string text, Action<string> test)
    {
        var dir = Path.Combine(Path.GetTempPath(), "tokenvampire-cli-" + Guid.NewGuid().ToString("N"));
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
