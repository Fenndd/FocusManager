using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class DashboardPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();

    private bool _isBusy;
    private bool _isUiUpdate;
    private bool _isAgentAvailable;
    private bool _lastKnownStudyMode;

    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshDashboardAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshDashboardAsync();
    }

    private async void StudyModeSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isUiUpdate || _isBusy)
        {
            return;
        }

        var requestedState = StudyModeSwitch.IsOn;

        if (!_isAgentAvailable)
        {
            SetStudyModeSwitchSilently(_lastKnownStudyMode);
            StatusText.Text = "Agent status: unavailable (click Refresh).";
            return;
        }

        _isBusy = true;
        ApplyControlAvailability();

        try
        {
            await _agentClient.SetStudyModeAsync(requestedState);

            _lastKnownStudyMode = requestedState;
            StatusText.Text = "Agent status: connected";
            ModeStatusText.Text = requestedState
                ? "Current mode: enabled"
                : "Current mode: disabled";
        }
        catch (Exception ex)
        {
            _isAgentAvailable = false;
            SetStudyModeSwitchSilently(_lastKnownStudyMode);
            StatusText.Text = $"Agent status: unavailable ({ex.Message})";
            ModeStatusText.Text = "Current mode: unknown";
        }
        finally
        {
            _isBusy = false;
            ApplyControlAvailability();
        }
    }

    private async Task RefreshDashboardAsync()
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        StatusText.Text = "Agent status: refreshing...";
        ApplyControlAvailability();

        try
        {
            var studyModeEnabled = await _agentClient.GetStudyModeStatusAsync();
            var whitelist = await _agentClient.GetWhitelistAsync();

            _isAgentAvailable = true;
            _lastKnownStudyMode = studyModeEnabled;

            SetStudyModeSwitchSilently(studyModeEnabled);

            StatusText.Text = "Agent status: connected";
            ModeStatusText.Text = studyModeEnabled
                ? "Current mode: enabled"
                : "Current mode: disabled";

            UpdateWhitelistCounters(whitelist);
        }
        catch (Exception ex)
        {
            _isAgentAvailable = false;
            StatusText.Text = $"Agent status: unavailable ({ex.Message})";
            ModeStatusText.Text = "Current mode: unknown";
            ReadinessText.Text = "Whitelist status: unavailable while agent is disconnected.";

            AppsButton.Content = "Applications (?)";
            FoldersButton.Content = "Folders (?)";
            SitesButton.Content = "Sites (?)";
        }
        finally
        {
            _isBusy = false;
            ApplyControlAvailability();
        }
    }

    private void UpdateWhitelistCounters(WhitelistConfig config)
    {
        var apps = config.AllowedApps.Count;
        var folders = config.AllowedFolders.Count;
        var sites = config.AllowedSites.Count;

        AppsButton.Content = $"Applications ({apps})";
        FoldersButton.Content = $"Folders ({folders})";
        SitesButton.Content = $"Sites ({sites})";

        if (apps == 0 || folders == 0 || sites == 0)
        {
            ReadinessText.Text = "Whitelist status: incomplete. Add at least one app, folder, and site before strict focus sessions.";
            return;
        }

        ReadinessText.Text = "Whitelist status: all categories configured.";
    }

    private void SetStudyModeSwitchSilently(bool value)
    {
        _isUiUpdate = true;
        try
        {
            StudyModeSwitch.IsOn = value;
        }
        finally
        {
            _isUiUpdate = false;
        }
    }

    private void ApplyControlAvailability()
    {
        RefreshButton.IsEnabled = !_isBusy;
        StudyModeSwitch.IsEnabled = !_isBusy && _isAgentAvailable;
    }

    private void OpenApps_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AppsWhitelistPage));
    }

    private void OpenFolders_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(FoldersWhitelistPage));
    }

    private void OpenSites_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SitesWhitelistPage));
    }
}
