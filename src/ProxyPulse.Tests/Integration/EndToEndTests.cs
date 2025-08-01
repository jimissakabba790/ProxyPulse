using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyPulse.Common.Configuration;
using ProxyPulse.Common.Models;
using ProxyPulse.Fetchers;
using ProxyPulse.Routing;
using ProxyPulse.SocksCore;
using System.Net;
using System.Text.Json;

namespace ProxyPulse.Tests.Integration;

public class EndToEndTests : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly TestProxyProvider _testProvider;
    private readonly HttpClient _httpClient;

    public EndToEndTests()
    {
        _testProvider = new TestProxyProvider();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppSettings>(settings =>
                {
                    settings.Providers = new List<ProviderConfig>
                    {
                        new() { Name = "Test", ApiKey = "test" }
                    };
                    settings.RefreshIntervalMinutes = 10;
                    settings.RoutingMode = "Global";
                });

                services.AddSingleton<IProxyProvider>(_testProvider);
                services.AddSingleton<ISocksService, SocksService>();
                services.AddSingleton<IRoutingService, RoutingService>();
            })
            .Build();

        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            Proxy = new WebProxy("socks5://localhost:1090")
        });
    }

    [Fact]
    public async Task Should_Route_Traffic_Through_Proxy()
    {
        // Arrange
        var socksService = _host.Services.GetRequiredService<ISocksService>();
        var routingService = _host.Services.GetRequiredService<IRoutingService>();

        await socksService.StartAsync(1090, CancellationToken.None);
        await routingService.EnableAsync(Array.Empty<string>(), CancellationToken.None);

        // Act
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonDocument>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        json.RootElement.GetProperty("origin").GetString()
            .Should().Be(_testProvider.Endpoints[0].Ip);

        // Cleanup
        await routingService.DisableAsync(CancellationToken.None);
        await socksService.StopAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        await _host.DisposeAsync();
        _httpClient.Dispose();
    }

    private class TestProxyProvider : IProxyProvider
    {
        public List<ProxyEndpoint> Endpoints { get; } = new()
        {
            new ProxyEndpoint("1.2.3.4", 1080, "test", "test")
        };

        public Task<IReadOnlyList<ProxyEndpoint>> FetchAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ProxyEndpoint>>(Endpoints);
        }
    }
}
