using System.Text;
using ClickstreamAnalytics.Provider;

namespace ClickstreamAnalytics.Storage
{
    public static class ClickstreamEventStorage
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
            var allEvents = GetAllEventsJson();
            if (eventsJson.Length == allEvents.Length)
            {
                ClickstreamPrefs.ClearData(EventKey);
            }
            else if (eventsJson.Length < allEvents.Length)
            {
                var newEvents = allEvents[(eventsJson.Length + 1)..];
                ClickstreamPrefs.SaveData(EventKey, newEvents);
            }
        }
    }
}