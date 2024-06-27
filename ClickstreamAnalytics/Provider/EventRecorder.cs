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

        public EventRecorder(ClickstreamContext context, MonoBehaviour mono)
        {
            _context = context;
            _mono = mono;
        }

        public void RecordEvents(Dictionary<string, object> eventDictionary)
        {
            var eventJson = ClickstreamJson.Serialize(eventDictionary);
            var saveResult = ClickstreamEventStorage.SaveEvent(eventJson);
            ClickstreamLog.Debug(eventJson);
            if (saveResult) return;
            ClickstreamLog.Info("Cache Capacity Reached, Sending Event Immediately");
            StartSendEvents(Event.Constants.Prefix + eventJson + Event.Constants.Suffix);
        }

        public void FlushEvents()
        {
            var events = ClickstreamEventStorage.GetAllEventsJson();
            if (events.Length <= 0) return;
            ClickstreamLog.Debug("start flush events");
            StartSendEvents(events);
        }

        private void StartSendEvents(string events)
        {
            _mono.StartCoroutine(NetRequest.SendRequest(events, _context, HandleWebRequestResult));
        }

        private static void HandleWebRequestResult(bool success, string response)
        {
            if (success)
            {
                ClickstreamLog.Debug("Send events successful!");
                ClickstreamEventStorage.ClearEvents(response);
            }
            else
            {
                ClickstreamLog.Error("Send events failed with error: " + response);
            }
        }
    }
}