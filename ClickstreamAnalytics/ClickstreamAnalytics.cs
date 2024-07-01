using System.Collections.Generic;
using ClickstreamAnalytics.Provider;
using ClickstreamAnalytics.Util;
using UnityEngine;

namespace ClickstreamAnalytics
{
    public static class ClickstreamAnalytics
    {
        private static ClickstreamProvider _provider;

        public static bool Init(ClickstreamConfiguration configuration)
        {
            if (_provider != null)
            {
                ClickstreamLog.Warn("Clickstream SDK has initialized");
                return false;
            }

            if (string.IsNullOrEmpty(configuration.AppId) || string.IsNullOrEmpty(configuration.Endpoint))
            {
                ClickstreamLog.Warn("Clickstream SDK initialization failed, AppId or Endpoint not configured");
                return false;
            }

            var mProviderAutoTrack = new GameObject("ClickstreamProvider", typeof(ClickstreamProvider));
            _provider = (ClickstreamProvider)mProviderAutoTrack.GetComponent(typeof(ClickstreamProvider));
            _provider.Configure(configuration);
            Object.DontDestroyOnLoad(mProviderAutoTrack);
            ClickstreamLog.Debug("Clickstream SDK initialized successful with configuration:\n" + configuration);
            return true;
        }

        public static void Record(string name, Dictionary<string, object> attributes = null)
        {
            _provider.Record(name, attributes);
        }

        public static void SetUserId(string userId)
        {
            _provider.SetUserId(userId);
        }

        public static void SetUserAttributes(Dictionary<string, object> userAttributes)
        {
            _provider.SetUserAttributes(userAttributes);
        }

        public static void SetGlobalAttributes(Dictionary<string, object> globalAttributes)
        {
            _provider.SetGlobalAttributes(globalAttributes);
        }

        public static void UpdateConfiguration(Configuration configuration)
        {
            _provider.UpdateConfiguration(configuration);
        }

        public static void FlushEvents()
        {
            _provider.FlushEvents();
        }
    }

    public class Configuration
    {
        public string AppId { get; set; }
        public string Endpoint { get; set; }
        public bool? IsLogEvents { get; set; }
        public bool? IsTrackAppStartEvents { get; set; }
        public bool? IsTrackAppEndEvents { get; set; }
        public bool? IsTrackSceneLoadEvents { get; set; }
        public bool? IsTrackSceneUnLoadEvents { get; set; }
        public bool? IsCompressEvents { get; set; }
    }

    public class ClickstreamConfiguration
    {
        public string AppId { get; set; }
        public string Endpoint { get; set; }
        public bool IsLogEvents { get; set; }
        public bool IsTrackAppStartEvents { get; set; }
        public bool IsTrackAppEndEvents { get; set; }
        public bool IsTrackSceneLoadEvents { get; set; }
        public bool IsTrackSceneUnLoadEvents { get; set; }
        public bool IsCompressEvents { get; set; }
        public int SendEventsInterval { get; set; } = 10000;

        public Dictionary<string, object> GlobalAttributes { get; set; }

        public override string ToString()
        {
            return
                $"AppId: {AppId},\n" +
                $"Endpoint: {Endpoint},\n" +
                $"IsLogEvents: {IsLogEvents},\n" +
                $"IsTrackAppStartEvents: {IsTrackAppStartEvents},\n" +
                $"IsTrackAppEndEvents: {IsTrackAppEndEvents},\n" +
                $"IsTrackSceneLoadEvents: {IsTrackSceneLoadEvents},\n" +
                $"IsTrackSceneUnLoadEvents: {IsTrackSceneUnLoadEvents},\n" +
                $"IsCompressEvents: {IsCompressEvents},\n" +
                $"SendEventsInterval: {SendEventsInterval}\n";
        }
    }
}