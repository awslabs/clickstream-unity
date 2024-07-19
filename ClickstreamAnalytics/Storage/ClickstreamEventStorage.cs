using System.Text;
using ClickstreamAnalytics.Provider;
using ClickstreamAnalytics.Util;

namespace ClickstreamAnalytics.Storage
{
    internal static class ClickstreamEventStorage
    {
        private const int MaxLength = 1024 * 1024;
        private static readonly StringBuilder MemoryEventBuilder = new();
        private static int _currentMemoryByteCount;

        public static bool SaveEvent(string eventJson)
        {
            var allEvents = GetAllEvents();
            var allByteCount = Encoding.UTF8.GetByteCount(eventJson) + Encoding.UTF8.GetByteCount(allEvents) + 1;
            if (allByteCount >= MaxLength)
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

        public static bool SaveEventInMemory(string eventJson)
        {
            var allByteCount = Encoding.UTF8.GetByteCount(eventJson) + 1;
            if (allByteCount + _currentMemoryByteCount >= MaxLength)
            {
                return false;
            }

            if (MemoryEventBuilder.Length > 0)
            {
                MemoryEventBuilder.Append(',');
                MemoryEventBuilder.Append(eventJson);
            }
            else
            {
                MemoryEventBuilder.Append(Event.Constants.Prefix);
                MemoryEventBuilder.Append(eventJson);
            }

            _currentMemoryByteCount += allByteCount + 1;
            return true;
        }

        private static string GetAllEvents()
        {
            var events = ClickstreamPrefs.GetData(ClickstreamPrefs.EventsKey, typeof(string)) ?? "";
            return (string)events;
        }

        public static string GetAllEventsJson()
        {
            if (MemoryEventBuilder.Length > 0)
            {
                return MemoryEventBuilder + Event.Constants.Suffix;
            }

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

        public static void ClearMemoryEvents(string eventsJson)
        {
            if (eventsJson.Length == MemoryEventBuilder.Length + 1)
            {
                MemoryEventBuilder.Clear();
                _currentMemoryByteCount = 0;
            }
            else
            {
                var allEvents = MemoryEventBuilder.ToString();
                var newEvents = Event.Constants.Prefix + allEvents[(eventsJson.Length)..];
                MemoryEventBuilder.Clear();
                MemoryEventBuilder.Append(newEvents);
                _currentMemoryByteCount = Encoding.UTF8.GetByteCount(newEvents);
            }
        }

        public static void SaveUnsentMemoryEvents()
        {
            if (MemoryEventBuilder.Length <= 0) return;
            SaveFailedEvents(MemoryEventBuilder + Event.Constants.Suffix);
            _currentMemoryByteCount = 0;
        }

        public static void SaveFailedEvents(string failedEventsJson)
        {
            var eventJson = failedEventsJson.Substring(1, failedEventsJson.Length - 2);
            var allEvents = GetAllFailedEvents();
            var allByteCount = Encoding.UTF8.GetByteCount(eventJson) + Encoding.UTF8.GetByteCount(allEvents) + 1;
            if (allByteCount >= MaxLength)
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