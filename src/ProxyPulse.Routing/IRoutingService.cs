namespace ProxyPulse.Routing;

public interface IRoutingService
{
    Task EnableAsync(IEnumerable<string> processNames, CancellationToken cancellationToken);
    Task DisableAsync(CancellationToken cancellationToken);
}
