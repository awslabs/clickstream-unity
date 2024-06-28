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
        private Dictionary<string, object> _userAttributes = new();
        private readonly Dictionary<string, object> _globalAttributes = new();

        public void Configure(ClickstreamConfiguration config)
        {
            ClickstreamLog.EnableLog(config.IsLogEvents);
            _configuration = config;
            _context = new ClickstreamContext(_configuration);
            _eventRecorder = new EventRecorder(_context, this);
            _userAttributes = ClickstreamUserStorage.GetSimpleUserAttributes();
            StartCoroutine(StartTimer());
            _eventRecorder.FlushFailedEvents();
        }

        public void Record(
            string eventName,
            Dictionary<string, object> attributes = null,
            Dictionary<string, object> allUserAttributes = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                ClickstreamLog.Error("Event name is invalid");
                return;
            }

            var eventDictionary = EventBuilder.CreatedEvent(_context, eventName, attributes,
                allUserAttributes ?? _userAttributes,
                _globalAttributes
            );

            _eventRecorder.RecordEvents(eventDictionary);
        }

        private void RecordProfileSet(Dictionary<string, object> userAttributes)
        {
            Record(Event.PresetEvents.ProfileSet, attributes: null, allUserAttributes: userAttributes);
        }


        public void SetUserId(string userId)
        {
            var previousUserId = "";
            if (_userAttributes.TryGetValue(Event.Attr.UserId, out var currentUserId))
            {
                previousUserId = currentUserId.ToString();
            }

            if (userId == null)
            {
                _userAttributes.Remove(Event.Attr.UserId);
            }
            else if (userId != previousUserId)
            {
                var userInfo = ClickstreamUserStorage.GetUserInfoFromMapping(userId);
                _userAttributes = new Dictionary<string, object>
                {
                    {
                        Event.Attr.UserId, new Dictionary<string, object>
                        {
                            { "value", userId },
                            { "set_timestamp", ClickstreamDate.GetCurrentTimestamp() },
                        }
                    },
                    { Event.Attr.FirstTouchTimestamp, userInfo[Event.Attr.FirstTouchTimestamp] }
                };
                _context.UserUniqueId = ClickstreamUserStorage.GetUserUniqueId();
            }

            ClickstreamUserStorage.SaveAllUserAttributes(_userAttributes);
            RecordProfileSet(_userAttributes);
        }

        public void SetUserAttributes(Dictionary<string, object> userAttributes)
        {
            var allUserAttributes = ClickstreamUserStorage.GetAllUserAttributes();
            var timestamp = ClickstreamDate.GetCurrentTimestamp();
            foreach (var kvp in userAttributes)
            {
                if (kvp.Value == null)
                {
                    allUserAttributes.Remove(kvp.Key);
                }
                else
                {
                    var userObject = new Dictionary<string, object>
                    {
                        { "value", kvp.Value },
                        { "set_timestamp", timestamp }
                    };
                    allUserAttributes[kvp.Key] = userObject;
                }
            }

            ClickstreamUserStorage.SaveAllUserAttributes(allUserAttributes);
            RecordProfileSet(allUserAttributes);
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