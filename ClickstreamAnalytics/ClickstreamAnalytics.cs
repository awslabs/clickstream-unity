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

            var mThinkingSDKAutoTrack = new GameObject("ClickstreamProvider", typeof(ClickstreamProvider));
            _provider = (ClickstreamProvider)mThinkingSDKAutoTrack.GetComponent(typeof(ClickstreamProvider));
            _provider.Configure(configuration);
            Object.DontDestroyOnLoad(mThinkingSDKAutoTrack);
            return true;
        }

        public static void Record(string name, Dictionary<string, object> attributes = null)
        {
            _provider.Record(name, attributes);
        }

        public static void FlushEvents()
        {
            _provider.FlushEvents();
        }
    }

    public class ClickstreamConfiguration
    {
        public string AppId { get; set; }
        public string Endpoint { get; set; }
        public bool IsLogEvents { get; set; } = false;

        public int SendEventsInterval { get; set; } = 5000;
    }
}