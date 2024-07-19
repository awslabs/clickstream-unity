using ClickstreamAnalytics.Storage;

namespace ClickstreamAnalytics.Provider
{
    internal class ClickstreamContext
    {
        public ClickstreamConfiguration Configuration { get; }
        public string UserUniqueId { get; set; }
        public DeviceInfo DeviceInfo { get; }

        public ClickstreamContext(ClickstreamConfiguration configuration)
        {
            Configuration = configuration;
            UserUniqueId = ClickstreamUserStorage.GetUserUniqueId();
            DeviceInfo = new DeviceInfo();
        }
    }
}