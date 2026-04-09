using System.Collections.ObjectModel;
using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

    private void BackToDashboard_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(DashboardPage));
    }

    private async void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.WindowInstance;
            if (window is null)
            {
                StatusText.Text = "Status: app window is not available for picker.";
                return;
            }

            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            picker.FileTypeFilter.Add(".exe");

            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }

            AppPathInput.Text = NormalizeExecutablePath(file.Path);

            if (string.IsNullOrWhiteSpace(AppDisplayNameInput.Text))
            {
                AppDisplayNameInput.Text = Path.GetFileNameWithoutExtension(file.Name);
            }

            StatusText.Text = "Status: file selected";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to open picker ({ex.Message})";
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var executablePath = NormalizeExecutablePath(AppPathInput.Text);

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            StatusText.Text = "Status: executable path is required.";
            return;
        }

        if (!executablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            StatusText.Text = "Status: executable path must point to .exe file.";
            return;
        }

        if (!File.Exists(executablePath))
        {
            StatusText.Text = "Status: executable file does not exist.";
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
            displayName = Path.GetFileNameWithoutExtension(executablePath);
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

    private static string NormalizeExecutablePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');

        try
        {
            cleaned = Path.GetFullPath(cleaned);
        }
        catch
        {
            // Keep raw value if normalization fails.
        }

        return cleaned;
    }

    private sealed class AppListItem
    {
        public string DisplayName { get; init; } = string.Empty;
        public string ExecutablePath { get; init; } = string.Empty;
    }
}
