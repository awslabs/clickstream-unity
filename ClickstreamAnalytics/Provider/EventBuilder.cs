using System;
using System.Collections.Generic;
using System.Linq;
using ClickstreamAnalytics.Util;
using UnityEngine;
using UnityEditor.PackageManager;

namespace ClickstreamAnalytics.Provider
{
    internal static class EventBuilder
    {
        private const string PackageName = "software.aws.clickstream";
        private static readonly string AppVersion = Application.version;
        private static readonly string AppPackageName = Application.identifier;
        private static readonly string SDKVersion = GetSDKVersion();

        public static Dictionary<string, object> CreatedEvent(
            ClickstreamContext context,
            string eventName,
            Dictionary<string, object> attributes,
            Dictionary<string, object> userAttributes,
            Dictionary<string, object> globalAttributes
        )
        {
            var deviceInfo = context.DeviceInfo;

            var eventAttributes = GetEventAttributesWithCheck(attributes, globalAttributes);

            var timestamp = ClickstreamDate.GetCurrentTimestamp();
            var eventDictionary = new Dictionary<string, object>
            {
                { "event_type", eventName },
                { "event_id", Guid.NewGuid().ToString() },
                { "device_id", deviceInfo.DeviceId },
                { "unique_id", context.UserUniqueId },
                { "app_id", context.Configuration.AppId },
                { "timestamp", timestamp },
                { "platform", deviceInfo.Platform },
                { "os_version", deviceInfo.OSVersion },
                { "make", deviceInfo.Manufacture },
                { "model", deviceInfo.DeviceModel },
                { "locale", deviceInfo.Locale },
                { "zone_offset", deviceInfo.ZoneOffset },
                { "network_type", DeviceInfo.NetworkType() },
                { "screen_width", deviceInfo.ScreenWidth },
                { "screen_height", deviceInfo.ScreenHeight },
                { "system_language", deviceInfo.Language },
                { "country_code", deviceInfo.Country },
                { "sdk_name", "aws-solution-clickstream-sdk" },
                { "sdk_version", SDKVersion },
                { "app_package_name", AppPackageName },
                { "app_version", AppVersion },
                { "user", userAttributes },
                { "attributes", eventAttributes }
            };
            return eventDictionary;
        }

        private static string GetSDKVersion()
        {
            var package =  UnityEditor.AssetDatabase.FindAssets("package")
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Where(x => UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
                .Select(PackageInfo.FindForAssetPath)
                .Where(x => x != null)
                .FirstOrDefault(x => x.name == PackageName);
            return package == null ? "" : package.version;
        }

        private static Dictionary<string, object> GetEventAttributesWithCheck(
            Dictionary<string, object> eventAttributes,
            Dictionary<string, object> globalAttributes)
        {
            var customAttributes = new Dictionary<string, object>();
            var globalAttributesCount = globalAttributes?.Count ?? 0;

            if (eventAttributes is { Count: > 0 })
            {
                foreach (var (key, value) in eventAttributes)
                {
                    if (value == null) continue;
                    var currentNumber = customAttributes.Count + globalAttributesCount;
                    var result = EventChecker.CheckAttributes(currentNumber, key, value);
                    if (result.ErrorCode > 0)
                    {
                        customAttributes[Event.Attr.ErrorCodeKey] = result.ErrorCode;
                        customAttributes[Event.Attr.ErrorMessageKey] = result.ErrorMessage;
                    }
                    else
                    {
                        customAttributes[key] = value;
                    }
                }
            }

            if (globalAttributes == null) return customAttributes;
            foreach (var kvp in globalAttributes.Where(kvp => !customAttributes.ContainsKey(kvp.Key)))
            {
                customAttributes[kvp.Key] = kvp.Value;
            }

            return customAttributes;
        }
    }
}