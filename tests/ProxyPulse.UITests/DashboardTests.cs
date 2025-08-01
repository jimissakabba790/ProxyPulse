using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Xunit;

namespace ProxyPulse.UITests;

public class DashboardTests : IClassFixture<ProxyPulseFixture>
{
    private readonly ProxyPulseFixture _fixture;

    public DashboardTests(ProxyPulseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_ShowsProxyListAndAllowsRouting()
    {
        var window = _fixture.MainWindow;
        
        // Open dashboard via tray
        var trayIcon = window.FindFirstDescendant("TrayIcon");
        var clickPoint = trayIcon.GetClickablePoint();
        Mouse.Click(clickPoint, MouseButton.Right);
        var menu = window.FindFirstChild("TrayContextMenu").AsMenu();
        menu.Items.First(i => i.Text == "Open Dashboard").Click();

        // Verify proxy grid
        var proxyGrid = window.FindFirstDescendant("ProxyGrid").AsDataGridView();
        Assert.True(proxyGrid.Rows != null && proxyGrid.Rows.Length > 0);
        
        var firstRow = proxyGrid.Rows[0];
        var nameCell = firstRow.Cells["Name"].AsGridCell();
        var nameValue = nameCell.Patterns.Value.Pattern.Value?.ToString();
        Assert.NotNull(nameValue);
        var ipPortCell = firstRow.Cells["IP:Port"].AsGridCell();
        var ipPortValue = ipPortCell.Patterns.Value.Pattern.Value?.ToString();
        Assert.Matches(@"\d+\.\d+\.\d+\.\d+:\d+", ipPortValue ?? "");
        var pingCell = firstRow.Cells["Ping"].AsGridCell();
        var pingValue = pingCell.Patterns.Value.Pattern.Value?.ToString();
        int ping;
        Assert.True(int.TryParse(pingValue ?? "0", out ping));
        var statusCell = firstRow.Cells["Status"].AsGridCell();
        var statusValue = statusCell.Patterns.Value.Pattern.Value?.ToString();
        Assert.NotNull(statusValue);

        // Verify process list 
        var processList = window.FindFirstDescendant("ProcessList").AsListBox();
        var processItems = processList.Items;
        Assert.True(processItems != null && processItems.Length > 0);
        var firstProcess = processItems[0];
        firstProcess.AsCheckBox().Toggle();

        var enableButton = window.FindFirstDescendant("EnableProxyButton").AsButton();
        enableButton.Click();

        // Verify routing enabled
        var routingStatus = window.FindFirstDescendant("RoutingStatus");
        var routingStatusValue = routingStatus.Patterns.Value.Pattern.Value?.ToString();
        Assert.Contains("Enabled", routingStatusValue ?? "");

        // Verify stats update
        var sparkline = window.FindFirstDescendant("TrafficSparkline");
        var initialDataStr = sparkline.Patterns.Value.Pattern.Value?.ToString();
        int initialData = 0;
        string cleanedData = string.IsNullOrEmpty(initialDataStr) ? "0" : initialDataStr.Replace(" ", "");
        Assert.True(int.TryParse(cleanedData, out initialData));        // Generate traffic
        using var client = new HttpClient(new SocketsHttpHandler 
        { 
            Proxy = new WebProxy("127.0.0.1:1080") 
        });
        await client.GetAsync("http://example.com");

        // Wait for stats update
        await Task.Delay(1000);
        var updatedDataStr = sparkline.Patterns.Value.Pattern.Value?.ToString();
        int updatedData = 0;
        string cleanedUpdatedData = string.IsNullOrEmpty(updatedDataStr) ? "0" : updatedDataStr.Replace(" ", "");
        Assert.True(int.TryParse(cleanedUpdatedData, out updatedData));
        Assert.NotEqual(initialData, updatedData);

        // Test Fetch Now
        var fetchButton = window.FindFirstDescendant("FetchNowButton").AsButton();
        var lastUpdate = window.FindFirstDescendant("LastUpdateTime");
        var initialTime = lastUpdate.Patterns.Value.Pattern.Value?.ToString();
        
        fetchButton.Click();
        await Task.Delay(1000);

        var newTime = lastUpdate.Patterns.Value.Pattern.Value?.ToString();
        Assert.NotEqual(initialTime, newTime);
    }
}
