using ProxyPulse.Common.Models;

namespace ProxyPulse.SocksCore;

public interface ISocksService
{
    Task StartAsync(int port, CancellationToken cancellationToken);
    Task UpdateEndpointsAsync(IEnumerable<ProxyEndpoint> endpoints, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
