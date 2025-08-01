using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ProxyPulse.UI.Services
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string Url { get; set; }
        public string Sha256 { get; set; }
        public string ReleaseNotes { get; set; }
        public string MinimumOSVersion { get; set; }
        public bool RequiresRestart { get; set; }
        public bool Mandatory { get; set; }
    }

    public class UpdateService
    {
        private readonly ILogger<UpdateService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _updateFeedUrl;
        private readonly string _currentVersion;

        public UpdateService(ILogger<UpdateService> logger, HttpClient httpClient, string updateFeedUrl)
        {
            _logger = logger;
            _httpClient = httpClient;
            _updateFeedUrl = updateFeedUrl;
            _currentVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
        }

        public async Task<UpdateInfo> CheckForUpdateAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(_updateFeedUrl);
                var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (Version.Parse(updateInfo.Version) > Version.Parse(_currentVersion))
                {
                    return updateInfo;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for updates");
                return null;
            }
        }

        public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                var tempFile = Path.Combine(Path.GetTempPath(), "ProxyPulse-Setup.exe");

                // Download the installer
                using (var response = await _httpClient.GetAsync(updateInfo.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var bytesRead = 0L;

                    using (var fileStream = File.Create(tempFile))
                    using (var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    {
                        var buffer = new byte[8192];
                        var read = 0;

                        while ((read = await downloadStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                            bytesRead += read;

                            if (totalBytes != -1L)
                            {
                                var progressPercentage = (int)((bytesRead * 100) / totalBytes);
                                progress?.Report(progressPercentage);
                            }
                        }
                    }
                }

                // Verify SHA256 checksum
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(tempFile))
                {
                    var hash = sha256.ComputeHash(stream);
                    var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                    if (hashString != updateInfo.Sha256.ToLowerInvariant())
                    {
                        _logger.LogError("Update package checksum verification failed");
                        File.Delete(tempFile);
                        return false;
                    }
                }

                // Launch installer
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempFile,
                    Arguments = "/SILENT /CLOSEAPPLICATIONS",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download and install update");
                return false;
            }
        }
    }
}
