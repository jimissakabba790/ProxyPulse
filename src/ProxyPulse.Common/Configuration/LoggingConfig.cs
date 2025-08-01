using System.Collections.Generic;

namespace ProxyPulse.Common.Configuration;

/// <summary>
/// Configuration options for ProxyPulse's logging system.
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// Gets or sets the log levels per category. Key is the category name (e.g. "Microsoft.AspNetCore"),
    /// value is the minimum log level (e.g. "Information", "Warning", "Error").
    /// </summary>
    public Dictionary<string, string> LogLevel { get; set; } = new();

    /// <summary>
    /// Gets or sets the file path for log output. Can include date/time placeholders like 'logs/ProxyPulse-.log'.
    /// </summary>
    public string FilePath { get; set; } = "logs/ProxyPulse.log";

    /// <summary>
    /// Gets or sets a value indicating whether to enable console logging.
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable file logging.
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL of a Seq server for structured logging. If null or empty, Seq logging is disabled.
    /// </summary>
    public string? SeqServerUrl { get; set; }
}
