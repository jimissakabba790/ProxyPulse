using FluentAssertions;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System.Diagnostics;

namespace ProxyPulse.Tests.UI;

public class WizardFlowTests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public WizardFlowTests()
    {
        // Launch the application
        _app = Application.Launch("src/ProxyPulse.UI/bin/Debug/net7.0-windows/ProxyPulse.UI.exe");
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
    }

    [Fact]
    public void Should_Complete_Setup_Wizard()
    {
        // Step 1: Provider Selection
        var providerCombo = _mainWindow.FindFirstDescendant("ProviderComboBox").AsComboBox();
        var apiKeyInput = _mainWindow.FindFirstDescendant("ApiKeyTextBox").AsTextBox();
        var nextButton = _mainWindow.FindFirstDescendant(cf => cf.ByText("Next")).AsButton();

        providerCombo.Select("Decodo");
        apiKeyInput.Text = "TEST_API_KEY";
        nextButton.Click();

        // Step 2: Refresh Interval
        var intervalSlider = _mainWindow.FindFirstDescendant("IntervalSlider").AsSlider();
        intervalSlider.Value.Should().Be(10.0);
        nextButton.Click();

        // Step 3: Routing Mode
        var perAppRadio = _mainWindow.FindFirstDescendant("PerAppRadio").AsRadioButton();
        var finishButton = _mainWindow.FindFirstDescendant(cf => cf.ByText("Finish")).AsButton();

        perAppRadio.Select();
        finishButton.Click();

        // Verify Dashboard
        var healthBadge = _mainWindow.FindFirstDescendant("HealthBadge").AsText();
        healthBadge.Text.Should().Be("Health: OK");
    }

    public void Dispose()
    {
        _automation.Dispose();
        if (!_app.HasExited)
        {
            _app.Kill();
        }
        _app.Dispose();
    }
}
