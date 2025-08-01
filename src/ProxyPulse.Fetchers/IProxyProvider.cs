using ProxyPulse.Common.Models;

namespace ProxyPulse.Fetchers;

public interface IProxyProvider
{
    Task<IReadOnlyList<ProxyEndpoint>> FetchAsync(CancellationToken cancellationToken);
}
