using System.Text;
using TokenVampire.I18n;

namespace TokenVampire.Reporting;

public static class ReportFile
{
    public static byte[] ToUtf8Bytes(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var lf = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        return new UTF8Encoding(false, true).GetBytes(UnicodeText.Nfc(lf));
    }
}
