using ClickstreamAnalytics.Storage;

namespace ClickstreamAnalytics.Provider
{
    internal class ClickstreamContext
    {
        private const string UserUniqueIDKey = ClickstreamPrefs.KeyPrefix + "userUniqueId";
        public ClickstreamConfiguration Configuration { get; }
        public string UserUniqueId { get; }
        public DeviceInfo DeviceInfo { get; }

        public ClickstreamContext(ClickstreamConfiguration configuration)
        {
            Configuration = configuration;
            UserUniqueId = GetUserUniqueId();
            DeviceInfo = new DeviceInfo();
        }

        private static string GetUserUniqueId()
        {
            var uniqueId = (string)(ClickstreamPrefs.GetData(UserUniqueIDKey, typeof(string)) ?? "");
            if (uniqueId != "") return uniqueId;
            uniqueId = System.Guid.NewGuid().ToString();
            ClickstreamPrefs.SaveData(UserUniqueIDKey, uniqueId);
            return uniqueId;
        }
    }
}