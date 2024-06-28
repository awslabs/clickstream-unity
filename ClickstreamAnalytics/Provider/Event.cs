namespace ClickstreamAnalytics.Provider
{
    internal static class Event
    {
        public static class Constants
        {
            public const string Prefix = "[";
            public const string Suffix = "]";
        }

        public static class PresetEvents
        {
            public const string FirstOpen = "_first_open";
            public const string AppStart = "_app_start";
            public const string AppEnd = "_app_end";
            public const string ProfileSet = "_profile_set";
            public const string ClickstreamError = "_clickstream_error";
        }

        public static class Attr
        {
            public const string UserId = "_user_id";
            public const string FirstTouchTimestamp = "_user_first_touch_timestamp";
            public const string ErrorCode = "_error_code";
            public const string ErrorMessage = "_error_message";
        }
    }
}