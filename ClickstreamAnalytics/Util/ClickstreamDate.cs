using System;

namespace ClickstreamAnalytics.Util
{
    internal static class ClickstreamDate
    {
        public static long GetCurrentTimestamp()
        {
            var now = DateTimeOffset.UtcNow;
            return now.ToUnixTimeMilliseconds();
        }
    }
}