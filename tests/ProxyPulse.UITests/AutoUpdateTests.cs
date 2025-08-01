using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Definitions;
using FlaUI.Core.Patterns;
using System;
using System.Drawing;
using Xunit;

namespace ProxyPulse.UITests;

public class AutoUpdateTests : IClassFixture<ProxyPulseFixture>
{
    private readonly ProxyPulseFixture _fixture;
    private readonly string _updateFeedPath;

    public AutoUpdateTests(ProxyPulseFixture fixture)
    {
        _fixture = fixture;
        _updateFeedPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "build", "update-feed.json");
    }

    [Fact]
    public async Task UpdateFlow_CompletesSuccessfully()
    {
        // Modify update feed
        var feed = @"{
            ""version"": ""1.0.1"",
            ""url"": ""https://github.com/jimissakabba790/ProxyPulse/releases/download/v1.0.1/ProxyPulse.exe"",
            ""sha256"": ""e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"",
            ""notes"": ""Test update""
        }";
        await File.WriteAllTextAsync(_updateFeedPath, feed);

        var window = _fixture.MainWindow;

        // Trigger check for updates
        var trayIcon = window.FindFirstDescendant("TrayIcon");
                    var clickPoint = trayIcon.GetClickablePoint();
            Mouse.Click(clickPoint, MouseButton.Right);
        var menu = window.FindFirstChild("TrayContextMenu").AsMenu();
        menu.Items.First(i => i.Text == "Check for Updates").Click();

        // Verify update banner
        var banner = window.FindFirstDescendant("UpdateBanner");
        Assert.NotNull(banner);
        Assert.Contains("1.0.1", banner.Patterns.Value.Pattern.Value);

        var installButton = banner.FindFirstDescendant("InstallButton").AsButton();
        installButton.Click();

        // Verify progress dialog
        var progressDialog = window.FindFirstDescendant("UpdateProgress");
        Assert.NotNull(progressDialog);

        // Wait for completion toast
        await Task.Delay(2000);
        var toast = window.FindFirstDescendant("UpdateCompleteToast");
        Assert.NotNull(toast);
        Assert.Contains("Restart to apply", toast.Patterns.Value.Pattern.Value);
    }
}
