using System.IO;
using System.Text;

namespace OpenRace.Extensions;

public static class StreamExtensions
{
    public static byte[] ToBytesWithPreamble(this string value, Encoding encoding)
    {
        using var stream = new MemoryStream();
        using var sw = new StreamWriter(stream, encoding);
        sw.Write(value);
        sw.Flush();
        return stream.ToArray();
    }
}