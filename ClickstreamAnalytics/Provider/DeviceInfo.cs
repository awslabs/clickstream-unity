using System;
using System.Globalization;
using ClickstreamAnalytics.Storage;
using UnityEngine;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace ClickstreamAnalytics.Provider
{
    internal class DeviceInfo
    {
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
        public string NetWorkType { get; private set; } = GetNetworkType();

        private static string GetDeviceId()
        {
            var deviceId = (string)(ClickstreamPrefs.GetData(ClickstreamPrefs.DeviceIDKey, typeof(string)) ?? "");
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
            ClickstreamPrefs.SaveData(ClickstreamPrefs.DeviceIDKey, deviceId);
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
                RuntimePlatform.PS4 => "PS4",
                RuntimePlatform.PS5 => "PS5",
                RuntimePlatform.Switch => "Switch",
                RuntimePlatform.VisionOS => "VisionOS",
                RuntimePlatform.XboxOne => "Xbox",
                RuntimePlatform.LinuxPlayer => "Linux",
                _ => "UNKNOWN"
            };
            return platform;
        }

        private static string GetNetworkType()
        {
            var networkType = Application.internetReachability switch
            {
                NetworkReachability.ReachableViaCarrierDataNetwork => "Mobile",
                NetworkReachability.ReachableViaLocalAreaNetwork => "WIFI",
                _ => "UNKNOWN"
            };
            return networkType;
        }

        public void UpdateNetworkType()
        {
            NetWorkType = GetNetworkType();
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