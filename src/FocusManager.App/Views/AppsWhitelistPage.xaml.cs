using System.Collections.ObjectModel;
using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class AppsWhitelistPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();
    private readonly ObservableCollection<AppListItem> _items = [];

    public AppsWhitelistPage()
    {
        InitializeComponent();
        AllowedAppsList.ItemsSource = _items;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = await _agentClient.GetWhitelistAsync();
            _items.Clear();

            foreach (var app in config.AllowedApps)
            {
                _items.Add(new AppListItem
                {
                    DisplayName = app.DisplayName,
                    ExecutablePath = app.ExecutablePath
                });
            }

            StatusText.Text = "Status: ready";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to load ({ex.Message})";
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var executablePath = AppPathInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            StatusText.Text = "Status: executable path is required.";
            return;
        }

        if (_items.Any(x => string.Equals(x.ExecutablePath, executablePath, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText.Text = "Status: this executable path is already in the whitelist.";
            return;
        }

        var displayName = AppDisplayNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = executablePath;
        }

        _items.Add(new AppListItem
        {
            DisplayName = displayName,
            ExecutablePath = executablePath
        });

        try
        {
            await SaveAppsToAgentAsync();
            AppDisplayNameInput.Text = string.Empty;
            AppPathInput.Text = string.Empty;
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (AllowedAppsList.SelectedItem is not AppListItem selected)
        {
            StatusText.Text = "Status: select an item to remove.";
            return;
        }

        _items.Remove(selected);

        try
        {
            await SaveAppsToAgentAsync();
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async Task SaveAppsToAgentAsync()
    {
        var config = await _agentClient.GetWhitelistAsync();
        config.AllowedApps = _items
            .Select(x => new AllowedApp(x.DisplayName, x.ExecutablePath))
            .ToList();

        await _agentClient.SaveWhitelistAsync(config);
    }

    private sealed class AppListItem
    {
        public string DisplayName { get; init; } = string.Empty;
        public string ExecutablePath { get; init; } = string.Empty;
    }
}
