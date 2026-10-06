using System.Text;

namespace TokenVampire.Infrastructure;

public sealed class IngestException(string message) : Exception(message);

public static class FileNamePolicy
{
    static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    const string Forbidden = "/\\:*?\"<>|";

    public static string Normalize(string name)
    {
        var n = name.Normalize(NormalizationForm.FormC);
        if (n.Length is 0 or > 255) throw new IngestException("Invalid file name length.");
        try { _ = new UTF8Encoding(false, true).GetByteCount(n); }
        catch (EncoderFallbackException) { throw new IngestException("Malformed Unicode in file name."); }

        foreach (var c in n)
        {
            if (char.IsControl(c) || Forbidden.Contains(c) || IsBidiControl(c))
                throw new IngestException("File name contains forbidden characters.");
        }

        if (n[^1] is '.' or ' ') throw new IngestException("File name ends with dot or space.");
        var dot = n.IndexOf('.');
        var stem = (dot < 0 ? n : n[..dot]).TrimEnd(' ');
        if (Reserved.Contains(stem)) throw new IngestException("Reserved device name.");
        return n;
    }

    static bool IsBidiControl(char c) =>
        c is (>= '\u202A' and <= '\u202E') or (>= '\u2066' and <= '\u2069') or '\u200E' or '\u200F' or '\u061C';
}
