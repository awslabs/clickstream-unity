namespace ClickstreamAnalytics.Util
{
    internal static class ClickstreamLog
    {
        private static bool _enableLog;

        public static void EnableLog(bool enabled)
        {
            _enableLog = enabled;
        }

        public static void Info(string message)
        {
            if (_enableLog)
            {
                UnityEngine.Debug.Log("[ClickstreamAnalytics] Info: " + message);
            }
        }

        public static void Debug(string message)
        {
            if (_enableLog)
            {
                UnityEngine.Debug.Log("[ClickstreamAnalytics] Debug: " + message);
            }
        }

        public static void Warn(string message)
        {
            if (_enableLog)
            {
                UnityEngine.Debug.LogWarning("[ClickstreamAnalytics] Warn: " + message);
            }
        }

        public static void Error(string message)
        {
            if (_enableLog)
            {
                UnityEngine.Debug.LogError("[ClickstreamAnalytics] Error: " + message);
            }
        }
    }
}