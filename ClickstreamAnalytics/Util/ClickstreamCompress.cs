using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ClickstreamAnalytics.Util
{
    internal static class ClickstreamCompress
    {
        public static string Gzip(string rawStr)
        {
            var inputBytes = Encoding.UTF8.GetBytes(rawStr);
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            var output = outputStream.ToArray();
            return Convert.ToBase64String(output);
        }
    }
}