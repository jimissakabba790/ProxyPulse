namespace ProxyPulse.Common.Configuration;

public class AppSettings
{
    public List<ProviderConfig> Providers { get; set; } = new();
    public int RefreshIntervalMinutes { get; set; } = 10;
    public string RoutingMode { get; set; } = "Per-App"; // or "Global"
}

public class ProviderConfig
{
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
}
