using FocusManager.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class DashboardPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();
    private bool _isLoading;

    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        try
        {
            var enabled = await _agentClient.GetStudyModeStatusAsync();
            StudyModeSwitch.IsOn = enabled;
            StatusText.Text = "Agent status: connected";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Agent status: unavailable ({ex.Message})";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void StudyModeSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        try
        {
            await _agentClient.SetStudyModeAsync(StudyModeSwitch.IsOn);
            StatusText.Text = "Agent status: connected";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Agent status: unavailable ({ex.Message})";
        }
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
