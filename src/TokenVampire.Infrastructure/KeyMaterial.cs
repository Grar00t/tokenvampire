namespace TokenVampire.Infrastructure;

public static class KeyMaterial
{
    const int SaltSize = 16;

    public static byte[] LoadOrCreateSalt(string directory)
    {
        Directory.CreateDirectory(directory);
        var p = Path.Combine(directory, "vault.salt");
        if (File.Exists(p))
        {
            var s = File.ReadAllBytes(p);
            return s.Length == SaltSize ? s : throw new IngestException("Corrupt salt file.");
        }
        var salt = Vault.NewSalt();
        using var fs = new FileStream(p, FileMode.CreateNew, FileAccess.Write);
        fs.Write(salt);
        return salt;
    }
}
