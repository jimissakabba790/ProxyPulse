using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Definitions;
using System;
using System.Drawing;
using Xunit;

namespace ProxyPulse.UITests;

public class SystemTrayTests : IClassFixture<ProxyPulseFixture>
{
    private readonly ProxyPulseFixture _fixture;

    public SystemTrayTests(ProxyPulseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TrayMenu_ContainsAllExpectedItems()
    {
        var window = _fixture.MainWindow;
        var trayIcon = window.FindFirstDescendant("TrayIcon");

        // Right click tray icon
            var clickPoint = trayIcon.GetClickablePoint();
            Mouse.Click(clickPoint, MouseButton.Right);        // Get menu items
        var menu = window.FindFirstChild("TrayContextMenu").AsMenu();
        var items = menu.Items;

        // Verify menu items
        var expected = new[]
        {
            "Enable/Disable Proxy",
            "Open Dashboard",
            "Check for Updates",
            "Send Support Request",
            "Exit"
        };

        foreach (var menuText in expected)
        {
            Assert.Contains(items, item => item.Text == menuText);
        }

        // Dismiss menu
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
    }
}
