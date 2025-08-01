using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.Net;
using FlaUI.Core.Definitions;
using Xunit;

namespace ProxyPulse.UITests;

public class SupportAndDiagnosticTests : IClassFixture<ProxyPulseFixture>
{
    private readonly ProxyPulseFixture _fixture;
    private readonly string _downloadsPath;

    public SupportAndDiagnosticTests(ProxyPulseFixture fixture)
    {
        _fixture = fixture;
        _downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
    }

    [Fact]
    public async Task SupportRequest_SubmitsSuccessfully()
    {
        var window = _fixture.MainWindow;

        // Open support dialog
        var trayIcon = window.FindFirstDescendant("TrayIcon");
                    var clickPoint = trayIcon.GetClickablePoint();
            Mouse.Click(clickPoint, MouseButton.Right);
        var menu = window.FindFirstChild("TrayContextMenu").AsMenu();
        menu.Items.First(i => i.Text == "Send Support Request").Click();

        // Fill in request
        var messageBox = window.FindFirstDescendant("SupportMessageBox").AsTextBox();
        messageBox.Text = "Test support";

        var includeLogsCheck = window.FindFirstDescendant("IncludeLogsCheckBox").AsCheckBox();
        includeLogsCheck.IsChecked = true;

        var sendButton = window.FindFirstDescendant("SendSupportButton").AsButton();
        sendButton.Click();

        // Verify toast
        await Task.Delay(1000);
        var toast = window.FindFirstDescendant("SupportSentToast");
        Assert.NotNull(toast);
        Assert.Contains("Support request sent", toast.Patterns.Value.Pattern.Value);

        // Verify telemetry event
        using var mockEndpoint = new HttpListener();
        mockEndpoint.Prefixes.Add("http://localhost:9999/");
        mockEndpoint.Start();

        var context = await mockEndpoint.GetContextAsync();
        var body = await new StreamReader(context.Request.InputStream).ReadToEndAsync();
        Assert.Contains("SupportRequestSubmitted", body);
    }

    [Fact]
    public void LogExport_GeneratesValidZip()
    {
        var window = _fixture.MainWindow;

        // Open settings
        var menuBar = window.FindFirstDescendant("MainMenu").AsMenu();
        menuBar.Items.First(i => i.Text == "Settings").Click();

        // Navigate to Logs & Diagnostics
        var settingsTree = window.FindFirstDescendant("SettingsTree").AsTree();
        var diagnosticsNode = settingsTree.FindFirstDescendant(cf => cf.ByName("Logs & Diagnostics"));
        diagnosticsNode.AsTreeItem().Select();

        var exportButton = window.FindFirstDescendant("ExportLogsButton").AsButton();
        exportButton.Click();

        // Verify zip file
        Thread.Sleep(1000); // Wait for file save
        var zipFile = Directory.GetFiles(_downloadsPath, "ProxyPulse_*.zip").OrderByDescending(f => File.GetCreationTime(f)).First();
        Assert.True(File.Exists(zipFile));

        // Check log contents
        using var zip = System.IO.Compression.ZipFile.OpenRead(zipFile);
        var logEntry = zip.GetEntry("ProxyPulse.log");
        Assert.NotNull(logEntry);

        using var reader = new StreamReader(logEntry.Open());
        var logContent = reader.ReadToEnd();
        
        // Verify required log entries
        Assert.Contains("AppStarted", logContent);
        Assert.Contains("FetchSucceeded", logContent);
        Assert.Contains("RoutingEnabled", logContent);
    }
}
