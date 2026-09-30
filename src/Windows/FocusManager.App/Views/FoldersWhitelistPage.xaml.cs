using System.Collections.ObjectModel;
using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace FocusManager.App.Views;

public sealed partial class FoldersWhitelistPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();
    private readonly ObservableCollection<FolderListItem> _items = [];
    private bool _isLoadingFolders;

    public FoldersWhitelistPage()
    {
        InitializeComponent();
        AllowedFoldersList.ItemsSource = _items;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _isLoadingFolders = true;
            var config = await _agentClient.GetWhitelistAsync();
            _items.Clear();

            foreach (var folder in config.AllowedFolders)
            {
                _items.Add(new FolderListItem
                {
                    DisplayName = folder.DisplayName,
                    FolderPath = folder.FolderPath,
                    AllowSubfolders = folder.AllowSubfolders
                });
            }

            StatusText.Text = "Status: ready";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to load ({ex.Message})";
        }
        finally
        {
            _isLoadingFolders = false;
        }
    }

    private void BackToDashboard_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(DashboardPage));
    }

    private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.WindowInstance;
            if (window is null)
            {
                StatusText.Text = "Status: app window is not available for picker.";
                return;
            }

            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            picker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }

            FolderPathInput.Text = NormalizeFolderPath(folder.Path);

            if (string.IsNullOrWhiteSpace(FolderDisplayNameInput.Text))
            {
                FolderDisplayNameInput.Text = folder.Name;
            }

            StatusText.Text = "Status: folder selected";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to open picker ({ex.Message})";
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var folderPath = NormalizeFolderPath(FolderPathInput.Text);

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            StatusText.Text = "Status: folder path is required.";
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            StatusText.Text = "Status: folder path does not exist.";
            return;
        }

        if (_items.Any(x => string.Equals(x.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText.Text = "Status: this folder path is already in the whitelist.";
            return;
        }

        var displayName = FolderDisplayNameInput.Text.Trim();
        var allowSubfolders = AllowSubfoldersInput.IsChecked ?? false;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = folderPath;
            }
        }

        _items.Add(new FolderListItem
        {
            DisplayName = displayName,
            FolderPath = folderPath,
            AllowSubfolders = allowSubfolders
        });

        try
        {
            await SaveFoldersToAgentAsync();
            FolderDisplayNameInput.Text = string.Empty;
            FolderPathInput.Text = string.Empty;
            AllowSubfoldersInput.IsChecked = false;
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (AllowedFoldersList.SelectedItem is not FolderListItem selected)
        {
            StatusText.Text = "Status: select an item to remove.";
            return;
        }

        _items.Remove(selected);

        try
        {
            await SaveFoldersToAgentAsync();
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async Task SaveFoldersToAgentAsync()
    {
        var config = await _agentClient.GetWhitelistAsync();
        config.AllowedFolders = _items
            .Select(x => new AllowedFolder(x.DisplayName, x.FolderPath, x.AllowSubfolders))
            .ToList();

        await _agentClient.SaveWhitelistAsync(config);
    }

    private async void RuleAllowSubfolders_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingFolders)
        {
            return;
        }

        try
        {
            await SaveFoldersToAgentAsync();
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private static string NormalizeFolderPath(string path)
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

        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private sealed class FolderListItem
    {
        public string DisplayName { get; init; } = string.Empty;
        public string FolderPath { get; init; } = string.Empty;
        public bool AllowSubfolders { get; set; }
    }
}
