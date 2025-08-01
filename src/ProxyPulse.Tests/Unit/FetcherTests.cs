using FluentAssertions;
using ProxyPulse.Common.Models;
using ProxyPulse.Fetchers;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ProxyPulse.Tests.Unit;

public class FetcherTests : IDisposable
{
    private readonly WireMockServer _mockServer;
    private readonly DecodoProvider _decodoProvider;

    public FetcherTests()
    {
        _mockServer = WireMockServer.Start();
        _decodoProvider = new DecodoProvider(new HttpClient
        {
            BaseAddress = new Uri(_mockServer.Urls[0])
        }, "test_api_key");
    }

    [Fact]
    public async Task FetchAsync_Should_Return_Valid_ProxyEndpoints()
    {
        // Arrange
        var expectedResponse = """
            [{"ip":"1.2.3.4","port":1080,"username":"u","password":"p"}]
            """;

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/proxies")
                .WithHeader("X-API-Key", "test_api_key")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(expectedResponse));

        // Act
        var result = await _decodoProvider.FetchAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var endpoint = result.First();
        endpoint.Ip.Should().Be("1.2.3.4");
        endpoint.Port.Should().Be(1080);
        endpoint.Username.Should().Be("u");
        endpoint.Password.Should().Be("p");
    }

    public void Dispose()
    {
        _mockServer.Stop();
        _mockServer.Dispose();
    }
}
