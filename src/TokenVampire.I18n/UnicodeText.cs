using System.Text;

namespace TokenVampire.I18n;

public static class UnicodeText
{
    public static string Nfc(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return s.Normalize(NormalizationForm.FormC);
    }

    public static string Isolate(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return "\u2068" + s + "\u2069";
    }

    public static bool ContainsBidiControl(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        foreach (var ch in s)
        {
            if ((ch >= '\u202A' && ch <= '\u202E') || (ch >= '\u2066' && ch <= '\u2069')
                || ch == '\u200E' || ch == '\u200F' || ch == '\u061C')
            {
                return true;
            }
        }
        return false;
    }
}
