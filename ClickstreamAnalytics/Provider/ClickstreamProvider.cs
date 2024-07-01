using System.Collections;
using System.Collections.Generic;
using ClickstreamAnalytics.Storage;
using ClickstreamAnalytics.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClickstreamAnalytics.Provider
{
    internal class ClickstreamProvider : MonoBehaviour
    {
        private ClickstreamConfiguration _configuration;
        private ClickstreamContext _context;
        private EventRecorder _eventRecorder;
        private Dictionary<string, object> _userAttributes = new();
        private readonly Dictionary<string, object> _globalAttributes = new();
        private bool _isEnded;

        public void Configure(ClickstreamConfiguration config)
        {
            ClickstreamLog.EnableLog(config.IsLogEvents);
            _configuration = config;
            _context = new ClickstreamContext(_configuration);
            _eventRecorder = new EventRecorder(_context, this);
            _userAttributes = ClickstreamUserStorage.GetSimpleUserAttributes();
            if (config.GlobalAttributes is { Count: > 0 })
            {
                SetGlobalAttributes(config.GlobalAttributes);
            }

            SetUpAutoTrack();
            StartCoroutine(StartTimer());
            _eventRecorder.FlushFailedEvents();
        }

        public void Record(
            string eventName,
            Dictionary<string, object> attributes = null,
            Dictionary<string, object> allUserAttributes = null)
        {
            var result = EventChecker.CheckEventName(eventName);
            if (result.ErrorCode > 0)
            {
                ClickstreamLog.Warn(result.ErrorMessage);
                RecordClickstreamError(result);
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
            if (userAttributes == null) return;
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
                    var currentNumber = allUserAttributes.Count;
                    var result = EventChecker.CheckUserAttribute(currentNumber, kvp.Key, kvp.Value);
                    if (result.ErrorCode > 0)
                    {
                        RecordClickstreamError(result);
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
            }

            ClickstreamUserStorage.SaveAllUserAttributes(allUserAttributes);
            RecordProfileSet(allUserAttributes);
        }

        public void SetGlobalAttributes(Dictionary<string, object> globalAttributes)
        {
            if (globalAttributes == null)
            {
                _globalAttributes.Clear();
                return;
            }

            foreach (var (key, value) in globalAttributes)
            {
                if (value == null)
                {
                    if (_globalAttributes.ContainsKey(key))
                    {
                        _globalAttributes.Remove(key);
                    }
                }
                else
                {
                    var result = EventChecker.CheckAttributes(_globalAttributes.Count, key, value);
                    if (result.ErrorCode > 0)
                    {
                        RecordClickstreamError(result);
                    }
                    else
                    {
                        _globalAttributes[key] = value;
                    }
                }
            }
        }

        private void RecordClickstreamError(EventError error)
        {
            Record(Event.PresetEvents.ClickstreamError,
                attributes: new Dictionary<string, object>
                {
                    { Event.Attr.ErrorCodeKey, error.ErrorCode },
                    { Event.Attr.ErrorMessageKey, error.ErrorMessage }
                });
        }

        private IEnumerator StartTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds((float)(_configuration.SendEventsInterval / 1000.0));
                FlushEvents();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public void UpdateConfiguration(Configuration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.AppId))
            {
                _configuration.AppId = configuration.AppId;
            }

            if (!string.IsNullOrEmpty(configuration.Endpoint))
            {
                _configuration.Endpoint = configuration.Endpoint;
            }

            if (configuration.IsLogEvents != null)
            {
                _configuration.IsLogEvents = (bool)configuration.IsLogEvents;
                ClickstreamLog.EnableLog(_configuration.IsLogEvents);
            }

            if (configuration.IsTrackAppStartEvents != null)
            {
                _configuration.IsTrackAppStartEvents = (bool)configuration.IsTrackAppStartEvents;
            }

            if (configuration.IsTrackAppEndEvents != null)
            {
                _configuration.IsTrackAppEndEvents = (bool)configuration.IsTrackAppEndEvents;
            }

            if (configuration.IsTrackSceneLoadEvents != null)
            {
                _configuration.IsTrackSceneLoadEvents = (bool)configuration.IsTrackSceneLoadEvents;
            }

            if (configuration.IsTrackSceneUnLoadEvents != null)
            {
                _configuration.IsTrackSceneUnLoadEvents = (bool)configuration.IsTrackSceneUnLoadEvents;
            }

            if (configuration.IsCompressEvents != null)
            {
                _configuration.IsCompressEvents = (bool)configuration.IsCompressEvents;
            }
        }

        public void FlushEvents()
        {
            _eventRecorder.FlushEvents();
        }

        // Unity Lifecycle methods
        private void OnApplicationFocus(bool hasFocus)
        {
            var shouldRecord = hasFocus ? _configuration.IsTrackAppStartEvents : _configuration.IsTrackAppEndEvents;
            if (!shouldRecord) return;
            if (hasFocus && _isEnded)
            {
                Record(Event.PresetEvents.AppStart);
            }
            else
            {
                Record(Event.PresetEvents.AppEnd);
                _isEnded = true;
            }
        }

        private void OnApplicationQuit()
        {
            if (Application.isFocused)
            {
                OnApplicationFocus(false);
            }
        }

        private void SetUpAutoTrack()
        {
            TrackFirstOpen();
            if (_configuration.IsTrackSceneLoadEvents)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            if (!_configuration.IsTrackSceneUnLoadEvents) return;
            SceneManager.sceneUnloaded -= OnSceneUnLoaded;
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_configuration.IsTrackSceneLoadEvents) return;
            var attributes = new Dictionary<string, object>
            {
                { Event.Attr.SceneName, scene.name },
                { Event.Attr.ScenePath, scene.path }
            };
            Record(Event.PresetEvents.SceneLoad, attributes);
        }

        private void OnSceneUnLoaded(Scene scene)
        {
            if (!_configuration.IsTrackSceneUnLoadEvents) return;
            var attributes = new Dictionary<string, object>
            {
                { Event.Attr.SceneName, scene.name },
                { Event.Attr.ScenePath, scene.path }
            };
            Record(Event.PresetEvents.SceneUnLoad, attributes);
        }

        private void TrackFirstOpen()
        {
            var isFirstOpenValue = (int)(ClickstreamPrefs.GetData(ClickstreamPrefs.IsFirstOpenKey, typeof(int)) ?? 0);
            if (isFirstOpenValue == 0)
            {
                Record(Event.PresetEvents.FirstOpen);
                ClickstreamPrefs.SaveData(ClickstreamPrefs.IsFirstOpenKey, 1);
            }

            Record(Event.PresetEvents.AppStart, new Dictionary<string, object>
            {
                { Event.Attr.IsFirstTime, true }
            });
        }
    }
}