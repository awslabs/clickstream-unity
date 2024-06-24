using ClickstreamAnalytics.Network;
using ClickstreamAnalytics.Storage;
using ClickstreamAnalytics.Util;
using UnityEngine;

namespace ClickstreamAnalytics.Provider
{
    public class EventRecorder
    {
        private readonly ClickstreamContext _context;
        private readonly MonoBehaviour _mono;

        public EventRecorder(ClickstreamContext context, MonoBehaviour mono)
        {
            _context = context;
            _mono = mono;
        }

        public void FlushEvents()
        {
            ClickstreamLog.Debug("start flush events");
            var events = ClickstreamEventStorage.GetAllEventsJson();
            if (events.Length > 0)
            {
                StartSendEvents(events);
            }
        }

        public void StartSendEvents(string events)
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