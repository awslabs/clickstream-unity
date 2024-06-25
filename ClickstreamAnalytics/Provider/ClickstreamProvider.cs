using System.Collections;
using System.Collections.Generic;
using ClickstreamAnalytics.Util;
using UnityEngine;

namespace ClickstreamAnalytics.Provider
{
    internal class ClickstreamProvider : MonoBehaviour
    {
        private ClickstreamConfiguration _configuration;
        private ClickstreamContext _context;
        private EventRecorder _eventRecorder;
        private readonly Dictionary<string, object> _userAttributes = new();
        private readonly Dictionary<string, object> _globalAttributes = new();

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

            var eventDictionary = EventBuilder.CreatedEvent(_context, eventName, attributes,
                _userAttributes,
                _globalAttributes
            );

            _eventRecorder.RecordEvents(eventDictionary);
        }

        public void SetUserAttributes(Dictionary<string, object> userAttributes)
        {
            var timestamp = ClickstreamDate.GetCurrentTimestamp();
            foreach (var kvp in userAttributes)
            {
                var userObject = new Dictionary<string, object>
                    { { "value", kvp.Value }, { "set_timestamp", timestamp } };
                _userAttributes[kvp.Key] = userObject;
            }
        }

        public void SetGlobalAttributes(Dictionary<string, object> globalAttributes)
        {
            foreach (var kvp in globalAttributes)
            {
                _globalAttributes[kvp.Key] = kvp.Value;
            }
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