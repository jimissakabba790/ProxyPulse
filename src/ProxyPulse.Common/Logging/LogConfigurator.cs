using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using ProxyPulse.Common.Configuration;

namespace ProxyPulse.Common.Logging;

/// <summary>
/// Provides extension methods to configure ProxyPulse's logging infrastructure.
/// </summary>
public static class LogConfigurator
{
    /// <summary>
    /// Configures ProxyPulse's logging infrastructure using Serilog.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseProxyPulseLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var loggingConfig = services.GetRequiredService<IOptions<LoggingConfig>>().Value;

            // Start with a base configuration
            loggerConfiguration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName();

            // Configure log levels from config
            foreach (var (category, level) in loggingConfig.LogLevel)
            {
                if (Enum.TryParse<LogEventLevel>(level, true, out var logLevel))
                {
                    loggerConfiguration.MinimumLevel.Override(category, logLevel);
                }
            }

            // Add console logging if enabled
            if (loggingConfig.EnableConsole)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Add file logging if enabled
            if (loggingConfig.EnableFile)
            {
                loggerConfiguration.WriteTo.File(
                    new CompactJsonFormatter(),
                    loggingConfig.FilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB per file
                    buffered: false,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));
            }

            // Add Seq logging if configured
            if (!string.IsNullOrWhiteSpace(loggingConfig.SeqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(loggingConfig.SeqServerUrl);
            }

            // Ensure all timestamps are in UTC
            loggerConfiguration.Destructure.ToMaximumDepth(4)
                             .Destructure.ToMaximumStringLength(1024)
                             .Destructure.ToMaximumCollectionCount(10);
        });
    }
}
