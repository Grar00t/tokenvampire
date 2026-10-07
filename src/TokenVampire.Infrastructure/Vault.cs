using System.Security.Cryptography;

namespace TokenVampire.Infrastructure;

public sealed class Vault(string directory, byte[] key)
{
    const int NonceSize = 12, TagSize = 16;
    const int HeaderSize = NonceSize + TagSize;

    public static byte[] DeriveKey(string passphrase, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, 600_000, HashAlgorithmName.SHA256, 32);

    public static byte[] NewSalt() => RandomNumberGenerator.GetBytes(16);

    public async Task PutAsync(string id, byte[] plain, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(plain);
        Directory.CreateDirectory(directory);

        var destination = PathFor(id);
        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag, Encoding(id));

        try
        {
            await File.WriteAllBytesAsync(temporary, [.. nonce, .. tag, .. cipher], ct);
            File.Move(temporary, destination, true);
        }
        finally
        {
            try
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
            catch (IOException)
            {
                // Best-effort cleanup must not mask the original write/move failure.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup must not mask the original write/move failure.
            }
        }
    }

    public async Task<byte[]> GetAsync(string id, CancellationToken ct = default)
    {
        var all = await File.ReadAllBytesAsync(PathFor(id), ct);
        if (all.Length < HeaderSize)
        {
            throw new CryptographicException("Vault record is truncated or corrupt.");
        }

        var nonce = all[..NonceSize];
        var tag = all[NonceSize..HeaderSize];
        var cipher = all[HeaderSize..];
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain, Encoding(id));
        return plain;
    }

    string PathFor(string id)
    {
        if (id.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("Invalid id.", nameof(id));
        return Path.Combine(directory, id + ".bin");
    }

    static byte[] Encoding(string id) => System.Text.Encoding.UTF8.GetBytes(id);
}
