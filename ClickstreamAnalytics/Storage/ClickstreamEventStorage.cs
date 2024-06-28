using System.Text;
using ClickstreamAnalytics.Provider;
using ClickstreamAnalytics.Util;

namespace ClickstreamAnalytics.Storage
{
    internal static class ClickstreamEventStorage
    {
        private const int MaxLength = 1024 * 1024;

        public static bool SaveEvent(string eventJson)
        {
            var allEvents = GetAllEvents();
            var allByteCount = Encoding.UTF8.GetByteCount(eventJson) + Encoding.UTF8.GetByteCount(allEvents);
            if (allByteCount > MaxLength)
            {
                return false;
            }

            if (allEvents.Length > 0)
            {
                ClickstreamPrefs.SaveData(ClickstreamPrefs.EventsKey, allEvents + ',' + eventJson);
            }
            else
            {
                ClickstreamPrefs.SaveData(ClickstreamPrefs.EventsKey, Event.Constants.Prefix + eventJson);
            }

            return true;
        }

        private static string GetAllEvents()
        {
            var events = ClickstreamPrefs.GetData(ClickstreamPrefs.EventsKey, typeof(string)) ?? "";
            return (string)events;
        }

        public static string GetAllEventsJson()
        {
            var events = GetAllEvents();
            return events.Length > 0 ? events + Event.Constants.Suffix : "";
        }

        public static void ClearEvents(string eventsJson)
        {
            var allEvents = GetAllEvents();
            if (eventsJson.Length == allEvents.Length + 1)
            {
                ClickstreamPrefs.ClearData(ClickstreamPrefs.EventsKey);
            }
            else
            {
                var newEvents = Event.Constants.Prefix + allEvents[(eventsJson.Length)..];
                ClickstreamPrefs.SaveData(ClickstreamPrefs.EventsKey, newEvents);
            }
        }

        public static void SaveFailedEvents(string failedEventsJson)
        {
            var eventJson = failedEventsJson.Substring(1, failedEventsJson.Length - 2);
            var allEvents = GetAllFailedEvents();
            var allByteCount = Encoding.UTF8.GetByteCount(eventJson) + Encoding.UTF8.GetByteCount(allEvents);
            if (allByteCount > MaxLength)
            {
                ClickstreamLog.Warn("No space to cache the failed events, and the failed events will be discarded");
                return;
            }

            if (allEvents.Length > 0)
            {
                ClickstreamPrefs.SaveData(ClickstreamPrefs.FailedEventsKey, allEvents + ',' + eventJson);
            }
            else
            {
                ClickstreamPrefs.SaveData(ClickstreamPrefs.FailedEventsKey, Event.Constants.Prefix + eventJson);
            }
        }

        private static string GetAllFailedEvents()
        {
            var events = ClickstreamPrefs.GetData(ClickstreamPrefs.FailedEventsKey, typeof(string)) ?? "";
            return (string)events;
        }

        public static string GetAllFailedEventsJson()
        {
            var events = GetAllFailedEvents();
            return events.Length > 0 ? events + Event.Constants.Suffix : "";
        }

        public static void ClearFailedEvents()
        {
            ClickstreamPrefs.ClearData(ClickstreamPrefs.FailedEventsKey);
        }
    }
}