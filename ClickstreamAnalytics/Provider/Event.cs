namespace ClickstreamAnalytics.Provider
{
    internal static class Event
    {
        public static class Limit
        {
            public const int MaxEventTypeLength = 50;
            public const int MaxNumOfAttributes = 500;
            public const int MaxNumOfUserAttributes = 100;
            public const int MaxLengthOfName = 50;
            public const int MaxLengthOfValue = 1024;
            public const int MaxLengthOfUserValue = 256;
            public const int MaxLengthOfErrorValue = 256;
        }

        public static class ErrorCode
        {
            public const int NoError = 0;
            public const int EventNameInvalid = 1001;
            public const int EventNameLengthExceed = 1002;
            public const int AttributeNameLengthExceed = 2001;
            public const int AttributeNameInvalid = 2002;
            public const int AttributeValueLengthExceed = 2003;
            public const int AttributeSizeExceed = 2004;
            public const int AttributeValueTypeInvalid = 2005;
            public const int UserAttributeSizeExceed = 3001;
            public const int UserAttributeNameLengthExceed = 3002;
            public const int UserAttributeNameInvalid = 3003;
            public const int UserAttributeValueLengthExceed = 3004;
            public const int UserAttributeValueTypeInvalid = 3005;
        }

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
            public const string SceneLoad = "_scene_load";
            public const string SceneUnLoad = "_scene_unload";
            public const string ProfileSet = "_profile_set";
            public const string ClickstreamError = "_clickstream_error";
        }

        public static class Attr
        {
            public const string UserId = "_user_id";
            public const string SceneName = "_scene_name";
            public const string ScenePath = "_scene_path";
            public const string IsFirstTime = "_is_first_time";
            public const string FirstTouchTimestamp = "_user_first_touch_timestamp";
            public const string ErrorCodeKey = "_error_code";
            public const string ErrorMessageKey = "_error_message";
        }
    }
}