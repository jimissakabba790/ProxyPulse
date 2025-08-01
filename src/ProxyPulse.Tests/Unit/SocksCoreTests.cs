using FluentAssertions;
using ProxyPulse.Common.Models;
using ProxyPulse.SocksCore;
using ProxyPulse.SocksCore.Selection;

namespace ProxyPulse.Tests.Unit;

public class SocksCoreTests
{
    private readonly List<ProxyEndpoint> _testEndpoints;
    private readonly SocksService _socksService;
    private readonly TestUpstreamSelector _selector;

    public SocksCoreTests()
    {
        _testEndpoints = new List<ProxyEndpoint>
        {
            new("10.0.0.1", 1080, "user1", "pass1"),
            new("10.0.0.2", 1080, "user2", "pass2")
        };

        _selector = new TestUpstreamSelector();
        _socksService = new SocksService(_selector);
    }

    [Fact]
    public async Task UpdateEndpointsAsync_Should_Update_Internal_List()
    {
        // Arrange
        await _socksService.StartAsync(1090, CancellationToken.None);

        // Act
        await _socksService.UpdateEndpointsAsync(_testEndpoints, CancellationToken.None);

        // Assert
        _selector.Endpoints.Should().BeEquivalentTo(_testEndpoints);

        await _socksService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void RoundRobinSelector_Should_Cycle_Through_Endpoints()
    {
        // Arrange
        var selector = new RoundRobinSelector();
        selector.UpdateEndpoints(_testEndpoints);

        // Act & Assert
        selector.GetNextEndpoint().Should().Be(_testEndpoints[0]);
        selector.GetNextEndpoint().Should().Be(_testEndpoints[1]);
        selector.GetNextEndpoint().Should().Be(_testEndpoints[0]); // Cycles back
    }

    private class TestUpstreamSelector : IUpstreamSelector
    {
        public List<ProxyEndpoint> Endpoints { get; private set; } = new();

        public ProxyEndpoint GetNextEndpoint() => Endpoints[0];

        public void UpdateEndpoints(IEnumerable<ProxyEndpoint> endpoints)
        {
            Endpoints = endpoints.ToList();
        }
    }
}
