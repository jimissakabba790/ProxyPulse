using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using System.Diagnostics;
using Xunit;

namespace ProxyPulse.UITests;

public class ProxyPulseFixture : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;
    private readonly string _appPath;

    public ProxyPulseFixture()
    {
        // Clean state for tests
        Directory.Delete(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProxyPulse"), 
            recursive: true);

        _appPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "..", "..", "..", "..", 
            "src", "ProxyPulse.UI", "bin", "Debug", "net7.0-windows",
            "ProxyPulse.exe");

        _app = Application.Launch(_appPath);
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
    }

    public void Dispose()
    {
        _automation?.Dispose();
        if (!_app.HasExited)
            _app.Kill();
        _app?.Dispose();
    }

    public Application App => _app;
    public UIA3Automation Automation => _automation;
    public Window MainWindow => _mainWindow;
}
