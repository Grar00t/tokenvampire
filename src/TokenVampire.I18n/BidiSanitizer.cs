using System.Text;

namespace TokenVampire.I18n;

public static class BidiSanitizer
{
    private static bool IsControl(char ch) =>
        (ch >= '\u202A' && ch <= '\u202E') || (ch >= '\u2066' && ch <= '\u2069')
        || ch == '\u200E' || ch == '\u200F' || ch == '\u061C';

    public static string Strip(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (!IsControl(ch))
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }
}
