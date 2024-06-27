using System.Text;
using ClickstreamAnalytics.Provider;

namespace ClickstreamAnalytics.Storage
{
    internal static class ClickstreamEventStorage
    {
        private const string EventKey = ClickstreamPrefs.KeyPrefix + "eventsKey";
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
                ClickstreamPrefs.SaveData(EventKey, allEvents + ',' + eventJson);
            }
            else
            {
                ClickstreamPrefs.SaveData(EventKey, Event.Constants.Prefix + eventJson);
            }

            return true;
        }

        private static string GetAllEvents()
        {
            var events = ClickstreamPrefs.GetData(EventKey, typeof(string)) ?? "";
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
                ClickstreamPrefs.ClearData(EventKey);
            }
            else
            {
                var newEvents = Event.Constants.Prefix + allEvents[(eventsJson.Length)..];
                ClickstreamPrefs.SaveData(EventKey, newEvents);
            }
        }
    }
}