using System;
using System.Collections.Generic;
using ClickstreamAnalytics.Provider;
using ClickstreamAnalytics.Util;

namespace ClickstreamAnalytics.Storage
{
    internal static class ClickstreamUserStorage
    {
        private static string _userUniqueId = "";

        public static string GetUserUniqueId()
        {
            if (_userUniqueId != "")
            {
                return _userUniqueId;
            }

            var uniqueId = GetCurrentUserUniqueId();
            if (uniqueId == null)
            {
                uniqueId = Guid.NewGuid().ToString();
                ClickstreamPrefs.SaveData(ClickstreamPrefs.UserUniqueIDKey, uniqueId);
                SaveUserFirstTouchTimestamp();
            }

            _userUniqueId = uniqueId;
            return uniqueId;
        }

        private static string GetCurrentUserUniqueId()
        {
            return (string)ClickstreamPrefs.GetData(ClickstreamPrefs.UserUniqueIDKey, typeof(string));
        }

        private static void SaveCurrentUserUniqueId(string uniqueId)
        {
            _userUniqueId = uniqueId;
            ClickstreamPrefs.SaveData(ClickstreamPrefs.UserUniqueIDKey, uniqueId);
        }

        private static void SaveUserFirstTouchTimestamp()
        {
            var firstTouchTimestamp = ClickstreamDate.GetCurrentTimestamp();
            var defaultUserAttributes = new Dictionary<string, object>
            {
                {
                    Event.Attr.FirstTouchTimestamp, new Dictionary<string, object>
                    {
                        { "value", firstTouchTimestamp },
                        { "set_timestamp", firstTouchTimestamp }
                    }
                }
            };
            SaveAllUserAttributes(defaultUserAttributes);
        }

        public static void SaveAllUserAttributes(Dictionary<string, object> userAttributes)
        {
            ClickstreamPrefs.SaveData(ClickstreamPrefs.UserAttributesKey, ClickstreamJson.Serialize(userAttributes));
        }

        public static Dictionary<string, object> GetAllUserAttributes()
        {
            var userAttributesStr =
                (string)ClickstreamPrefs.GetData(ClickstreamPrefs.UserAttributesKey, typeof(string));
            return ClickstreamJson.Deserialize(userAttributesStr);
        }

        public static Dictionary<string, object> GetSimpleUserAttributes()
        {
            var allUserAttributes = GetAllUserAttributes();

            var simpleUserAttributes = new Dictionary<string, object>
            {
                { Event.Attr.FirstTouchTimestamp, allUserAttributes[Event.Attr.FirstTouchTimestamp] }
            };
            if (allUserAttributes.TryGetValue(Event.Attr.UserId, out var attribute))
            {
                simpleUserAttributes[Event.Attr.UserId] = attribute;
            }

            return simpleUserAttributes;
        }

        private static void SaveUserIdMapping(Dictionary<string, object> userIdMapping)
        {
            ClickstreamPrefs.SaveData(ClickstreamPrefs.UserIdMappingKey, ClickstreamJson.Serialize(userIdMapping));
        }

        private static Dictionary<string, object> GetUserIdMapping()
        {
            var userIdMappingString =
                (string)ClickstreamPrefs.GetData(ClickstreamPrefs.UserIdMappingKey, typeof(string));
            return ClickstreamJson.Deserialize(userIdMappingString);
        }

        public static Dictionary<string, object> GetUserInfoFromMapping(string userId)
        {
            var userIdMapping = GetUserIdMapping();
            Dictionary<string, object> userInfo;
            var timestamp = ClickstreamDate.GetCurrentTimestamp();
            if (userIdMapping == null)
            {
                userIdMapping = new Dictionary<string, object>();
                userInfo = new Dictionary<string, object>
                {
                    {
                        "user_uniqueId", new Dictionary<string, object>
                        {
                            { "value", GetUserUniqueId() },
                            { "set_timestamp", timestamp }
                        }
                    },
                    {
                        Event.Attr.FirstTouchTimestamp, GetAllUserAttributes()[Event.Attr.FirstTouchTimestamp]
                    }
                };
            }
            else if (userIdMapping.TryGetValue(userId, out var value))
            {
                userInfo = (Dictionary<string, object>)value;
                SaveCurrentUserUniqueId(((Dictionary<string, object>)userInfo["user_uniqueId"])["value"].ToString());
            }
            else
            {
                var userUniqueId = Guid.NewGuid().ToString();
                SaveCurrentUserUniqueId(userUniqueId);
                userInfo = new Dictionary<string, object>
                {
                    {
                        "user_uniqueId", new Dictionary<string, object>
                        {
                            { "value", userUniqueId },
                            { "set_timestamp", timestamp }
                        }
                    },
                    {
                        Event.Attr.FirstTouchTimestamp, new Dictionary<string, object>
                        {
                            { "value", timestamp },
                            { "set_timestamp", timestamp }
                        }
                    }
                };
            }

            userIdMapping[userId] = userInfo;
            SaveUserIdMapping(userIdMapping);
            return userInfo;
        }
    }
}