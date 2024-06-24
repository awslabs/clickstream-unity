namespace ClickstreamAnalytics.Provider
{
    public class ClickstreamContext
    {
        public ClickstreamConfiguration Configuration { get; set; }
        public string UserUniqueId { get; set; }

        public ClickstreamContext(ClickstreamConfiguration configuration)
        {
            Configuration = configuration;
            UserUniqueId = System.Guid.NewGuid().ToString();
        }
    }
}