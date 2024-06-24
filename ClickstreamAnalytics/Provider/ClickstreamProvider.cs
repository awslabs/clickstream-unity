using System;
using System.Collections;
using System.Collections.Generic;
using ClickstreamAnalytics.Storage;
using ClickstreamAnalytics.Util;
using UnityEngine;

namespace ClickstreamAnalytics.Provider
{
    internal class ClickstreamProvider : MonoBehaviour
    {
        private ClickstreamConfiguration _configuration;
        private ClickstreamContext _context;
        private EventRecorder _eventRecorder;

        public void Configure(ClickstreamConfiguration config)
        {
            ClickstreamLog.EnableLog(config.IsLogEvents);
            _configuration = config;
            _context = new ClickstreamContext(_configuration);
            _eventRecorder = new EventRecorder(_context, this);
            ClickstreamLog.Info("finish configure");
            StartCoroutine(StartTimer());
        }

        public void Record(string eventName, Dictionary<string, object> attributes = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                ClickstreamLog.Error("Event name is invalid");
                return;
            }
            attributes ??= new Dictionary<string, object>();

            var now = DateTimeOffset.UtcNow;
            var timestamp = now.ToUnixTimeMilliseconds();

            var data = new Dictionary<string, object>
            {
                { "event_type", eventName },
                { "event_id", Guid.NewGuid().ToString() },
                { "time_stamp", timestamp },
                { "device_id", Guid.NewGuid().ToString() },
                { "unique_id", _context.UserUniqueId },
                { "attributes", attributes }
            };

            var eventJson = ClickstreamJson.Serialize(data);
            var saveResult = ClickstreamEventStorage.SaveEvent(eventJson);
            ClickstreamLog.Debug(eventJson);
            if (saveResult) return;
            ClickstreamLog.Info("Cache Capacity Reached, Sending Event Immediately");
            _eventRecorder.StartSendEvents(Event.Constants.Prefix + eventJson + Event.Constants.Suffix);
        }

        private IEnumerator StartTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds((float)(_configuration.SendEventsInterval / 1000.0));
                FlushEvents();
            }
        }

        public void FlushEvents()
        {
            _eventRecorder.FlushEvents();
        }
    }
}