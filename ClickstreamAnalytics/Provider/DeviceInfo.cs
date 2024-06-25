using System;
using System.Globalization;
using ClickstreamAnalytics.Storage;
using UnityEngine;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace ClickstreamAnalytics.Provider
{
    internal class DeviceInfo
    {
        private const string DeviceIDKey = ClickstreamPrefs.KeyPrefix + "deviceId";
        public string DeviceId { get; } = GetDeviceId();
        public int ScreenWidth { get; } = Screen.currentResolution.width;
        public int ScreenHeight { get; } = Screen.currentResolution.height;
        public string Manufacture { get; } = SystemInfo.graphicsDeviceVendor;
        public string DeviceModel { get; } = SystemInfo.deviceModel;
        public string Platform { get; } = GetPlatform();
        public string OSVersion { get; } = SystemInfo.operatingSystem;
        public int ZoneOffset { get; } = GetZoneOffsite();
        public string Locale { get; } = GetLocale();
        public string Country { get; } = GetCountry();
        public string Language { get; } = GetLanguage();

        private static string GetDeviceId()
        {
            var deviceId = (string)(ClickstreamPrefs.GetData(DeviceIDKey, typeof(string)) ?? "");
            if (deviceId != "") return deviceId;
#if (UNITY_WEBGL)
            deviceId = Guid.NewGuid().ToString();
#else
            deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
            }
#endif
            ClickstreamPrefs.SaveData(DeviceIDKey, deviceId);
            return deviceId;
        }

        private static string GetPlatform()
        {
            var platform = Application.platform switch
            {
                RuntimePlatform.Android => "Android",
                RuntimePlatform.IPhonePlayer => "iOS",
                RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => "Mac",
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => "Windows",
                RuntimePlatform.LinuxPlayer => "Linux",
                _ => "UNKNOWN"
            };
            return platform;
        }

        public static string NetworkType()
        {
            var networkType = Application.internetReachability switch
            {
                NetworkReachability.ReachableViaCarrierDataNetwork => "Mobile",
                NetworkReachability.ReachableViaLocalAreaNetwork => "WIFI",
                _ => "UNKNOWN"
            };
            return networkType;
        }

        private static int GetZoneOffsite()
        {
            var localZone = TimeZoneInfo.Local;
            var offset = localZone.GetUtcOffset(DateTime.Now);
            return (int)offset.TotalMilliseconds;
        }


        private static string GetLocale()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            return currentCulture.Name;
        }

        private static string GetCountry()
        {
            var locale = GetLocale();
            var localeParts = locale.Split('-');
            return localeParts.Length > 1 ? localeParts[1] : "UNKNOWN";
        }

        private static string GetLanguage()
        {
            var locale = GetLocale();
            var localeParts = locale.Split('-');
            return localeParts.Length > 0 ? localeParts[0] : "UNKNOWN";
        }
    }
}