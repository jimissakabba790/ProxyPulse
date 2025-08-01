using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Conditions;
using FlaUI.Core.Patterns;
using System.Drawing;
using System;
using FlaUI.Core.Definitions;
using Xunit;

namespace ProxyPulse.UITests;

public class SetupWizardTests : IClassFixture<ProxyPulseFixture>
{
    private readonly ProxyPulseFixture _fixture;

    public SetupWizardTests(ProxyPulseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void FirstRunSetupWizard_CompletesSuccessfully()
    {
        var window = _fixture.MainWindow;

        // Step 1: Provider & API Key
        var providerCombo = window.FindFirstDescendant("ProviderComboBox").AsComboBox();
        providerCombo.Select("Decodo");

        var apiKeyBox = window.FindFirstDescendant("ApiKeyTextBox").AsTextBox();
        apiKeyBox.Text = "TEST_KEY";

        var testButton = window.FindFirstDescendant("TestKeyButton").AsButton();
        testButton.Click();

        // Wait for success indicator
        Assert.True(WaitForElement(window, "SuccessIcon"));

        var nextButton = window.FindFirstDescendant("NextButton").AsButton();
        Assert.True(nextButton.IsEnabled);
        nextButton.Click();

        // Step 2: Refresh & Mode
        var intervalSpinner = window.FindFirstDescendant("RefreshIntervalSpinner").AsSpinner();
        Assert.Equal(10, intervalSpinner.Value);

        var perAppRadio = window.FindFirstDescendant("PerAppRadio").AsRadioButton();
        perAppRadio.Click();
        nextButton.Click();

        // Step 3: Finish
        var finishButton = window.FindFirstDescendant("FinishButton").AsButton();
        finishButton.Click();

        // Verify dashboard appears
        Assert.True(WaitForElement(window, "DashboardView"));

        // Verify tray icon
        var trayIcon = window.FindFirstDescendant("TrayIcon");
        Assert.NotNull(trayIcon);
        var color = trayIcon.Patterns.Value.Pattern.Value;
        Assert.Equal("Green", color);
    }

    private bool WaitForElement(Window window, string automationId, int timeoutMs = 5000)
    {
        var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
        while (DateTime.Now < endTime)
        {
            var element = window.FindFirstDescendant(automationId);
            if (element != null)
                return true;
            Thread.Sleep(100);
        }
        return false;
    }
}
