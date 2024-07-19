using System.Collections.Generic;
using ClickstreamAnalytics.Network;
using ClickstreamAnalytics.Storage;
using ClickstreamAnalytics.Util;
using UnityEngine;

namespace ClickstreamAnalytics.Provider
{
    internal class EventRecorder
    {
        private readonly ClickstreamContext _context;
        private readonly MonoBehaviour _mono;
        private int _bundleSequenceId;
        private bool _haveFailedEvents;

        public EventRecorder(ClickstreamContext context, MonoBehaviour mono)
        {
            _context = context;
            _mono = mono;
            _bundleSequenceId = GetSequenceId();
        }

        public void RecordEvents(Dictionary<string, object> eventDictionary)
        {
            var eventJson = ClickstreamJson.Serialize(eventDictionary);
            var saveResult = _context.Configuration.IsUseMemoryCache
                ? ClickstreamEventStorage.SaveEventInMemory(eventJson)
                : ClickstreamEventStorage.SaveEvent(eventJson);

            ClickstreamLog.Debug(eventJson);
            if (saveResult) return;
            ClickstreamLog.Info("Cache capacity reached, sending events immediately");
            StartSendEvents(Event.Constants.Prefix + eventJson + Event.Constants.Suffix, SendType.Realtime);
        }

        public void FlushEvents()
        {
            var events = ClickstreamEventStorage.GetAllEventsJson();
            if (events.Length <= 0) return;
            ClickstreamLog.Debug("Start flush events");
            StartSendEvents(events);
        }

        public void FlushFailedEvents()
        {
            var events = ClickstreamEventStorage.GetAllFailedEventsJson();
            _haveFailedEvents = events.Length > 0;
            if (!_haveFailedEvents) return;
            ClickstreamLog.Debug("Start flush failed events");
            StartSendEvents(events, SendType.FailedEvents);
        }

        private void StartSendEvents(string events, SendType sendType = SendType.Normal)
        {
            _mono.StartCoroutine(NetRequest.SendRequest(events, _context, _bundleSequenceId, sendType,
                HandleWebRequestResult));
            PlusSequenceId();
        }

        private void HandleWebRequestResult(bool success, string eventsJson, string errorMessage, SendType sendType)
        {
            switch (sendType)
            {
                case SendType.Normal:
                    if (success)
                    {
                        if (_context.Configuration.IsUseMemoryCache)
                        {
                            ClickstreamEventStorage.ClearMemoryEvents(eventsJson);
                        }
                        else
                        {
                            ClickstreamEventStorage.ClearEvents(eventsJson);
                        }

                        ClickstreamLog.Debug("Send events successful!");
                        if (_haveFailedEvents)
                        {
                            FlushFailedEvents();
                        }
                    }
                    else
                    {
                        ClickstreamLog.Warn("Send events failed with error: " + errorMessage);
                    }

                    break;
                case SendType.Realtime:
                    if (success)
                    {
                        ClickstreamLog.Debug("Send realtime events successful!");
                    }
                    else
                    {
                        ClickstreamEventStorage.SaveFailedEvents(eventsJson);
                        _haveFailedEvents = true;
                        ClickstreamLog.Warn("Send realtime events failed with error: " + errorMessage);
                    }

                    break;
                case SendType.FailedEvents:
                    if (success)
                    {
                        _haveFailedEvents = false;
                        ClickstreamEventStorage.ClearFailedEvents();
                        ClickstreamLog.Debug("Send failed events successful!");
                    }

                    break;
                default:
                    ClickstreamLog.Debug("SendType not found");
                    break;
            }
        }

        private void PlusSequenceId()
        {
            _bundleSequenceId += 1;
            ClickstreamPrefs.SaveData(ClickstreamPrefs.BundleSequenceIdKey, _bundleSequenceId);
        }

        private static int GetSequenceId()
        {
            var sequenceId = (int)(ClickstreamPrefs.GetData(ClickstreamPrefs.BundleSequenceIdKey, typeof(int)) ?? 0);
            if (sequenceId == 0)
            {
                sequenceId = 1;
            }

            return sequenceId;
        }
    }

    public enum SendType
    {
        Normal,
        Realtime,
        FailedEvents
    }
}