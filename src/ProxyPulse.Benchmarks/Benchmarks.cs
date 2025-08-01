using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using ProxyPulse.Routing;
using ProxyPulse.SocksCore;

namespace ProxyPulse.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}

[MemoryDiagnoser]
public class Benchmarks
{
    private IServiceProvider? _services;
    private ISocksService? _socksService;
    private IRoutingService? _routingService;
    private MockSocksEndpoint? _mockEndpoint;
    private HttpClient? _httpClient;

    [GlobalSetup]
    public void Setup()
    {
        // Set up DI
        var services = new ServiceCollection();
        services.AddSocksCore();
        services.AddRouting();
        _services = services.BuildServiceProvider();

        // Get services
        _socksService = _services.GetRequiredService<ISocksService>();
        _routingService = _services.GetRequiredService<IRoutingService>();

        // Start mock endpoint
        _mockEndpoint = new MockSocksEndpoint();
        _mockEndpoint.Start();

        // Set up routing in global mode
        _routingService.Initialize(new RoutingConfig 
        { 
            Mode = RoutingMode.Global,
            ProxyEndpoint = new ProxyEndpoint("127.0.0.1", _mockEndpoint.Port)
        });

        // Configure HTTP client
        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            Proxy = new WebProxy($"socks5://127.0.0.1:{_mockEndpoint.Port}")
        });
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _mockEndpoint?.Dispose();
        _routingService?.Dispose();
        _httpClient?.Dispose();
        (_services as IDisposable)?.Dispose();
    }

    [Benchmark]
    public async Task HandshakeBenchmark()
    {
        for (int i = 0; i < 1000; i++)
        {
            var client = _socksService!.CreateClient();
            await client.ConnectAsync("127.0.0.1", _mockEndpoint!.Port);
            await client.HandshakeAsync();
            await client.DisposeAsync();
        }
    }

    [Benchmark]
    public async Task HttpThroughputBenchmark()
    {
        const string testUrl = "http://localhost:8080";
        var tasks = new Task[100];
        
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = _httpClient!.GetAsync(testUrl);
        }
        
        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Minimal SOCKS5 endpoint for benchmark purposes.
/// Accepts connections and performs handshakes but doesn't forward traffic.
/// </summary>
internal sealed class MockSocksEndpoint : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts;
    private Task? _acceptTask;

    public int Port { get; }

    public MockSocksEndpoint()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _cts = new CancellationTokenSource();
    }

    public void Start()
    {
        _acceptTask = AcceptConnectionsAsync(_cts.Token);
    }

    private async Task AcceptConnectionsAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(client, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        try
        {
            using (client)
            {
                var stream = client.GetStream();
                
                // Auth method selection
                var buffer = new byte[257];
                await stream.ReadAsync(buffer.AsMemory(0, 2), ct);
                int methodCount = buffer[1];
                await stream.ReadAsync(buffer.AsMemory(0, methodCount), ct);
                await stream.WriteAsync(new byte[] { 0x05, 0x00 }, ct); // No auth

                // Connect request
                await stream.ReadAsync(buffer.AsMemory(0, 4), ct);
                int addrType = buffer[3];
                int addrLen = addrType switch
                {
                    1 => 4,   // IPv4
                    3 => await ReadByteAsync(stream, ct), // Domain
                    4 => 16,  // IPv6
                    _ => throw new ProtocolViolationException($"Invalid address type: {addrType}")
                };

                await stream.ReadAsync(buffer.AsMemory(0, addrLen + 2), ct); // Skip address+port

                // Success response
                await stream.WriteAsync(new byte[] 
                { 
                    0x05, 0x00, 0x00, 0x01, 
                    0, 0, 0, 0,  // IP
                    0, 0         // Port
                }, ct);
            }
        }
        catch when (ct.IsCancellationRequested)
        {
            // Shutdown requested
        }
        catch
        {
            // Ignore protocol errors in mock
        }
    }

    private static async Task<byte> ReadByteAsync(NetworkStream stream, CancellationToken ct)
    {
        var buffer = new byte[1];
        await stream.ReadAsync(buffer.AsMemory(), ct);
        return buffer[0];
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _cts.Dispose();
    }
}
