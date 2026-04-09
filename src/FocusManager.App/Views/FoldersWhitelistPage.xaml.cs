using System.Collections.ObjectModel;
using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class FoldersWhitelistPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();
    private readonly ObservableCollection<FolderListItem> _items = [];

    public FoldersWhitelistPage()
    {
        InitializeComponent();
        AllowedFoldersList.ItemsSource = _items;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = await _agentClient.GetWhitelistAsync();
            _items.Clear();

            foreach (var folder in config.AllowedFolders)
            {
                _items.Add(new FolderListItem
                {
                    DisplayName = folder.DisplayName,
                    FolderPath = folder.FolderPath
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
        var folderPath = FolderPathInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            StatusText.Text = "Status: folder path is required.";
            return;
        }

        if (_items.Any(x => string.Equals(x.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText.Text = "Status: this folder path is already in the whitelist.";
            return;
        }

        var displayName = FolderDisplayNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = folderPath;
        }

        _items.Add(new FolderListItem
        {
            DisplayName = displayName,
            FolderPath = folderPath
        });

        try
        {
            await SaveFoldersToAgentAsync();
            FolderDisplayNameInput.Text = string.Empty;
            FolderPathInput.Text = string.Empty;
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
            .Select(x => new AllowedFolder(x.DisplayName, x.FolderPath))
            .ToList();

        await _agentClient.SaveWhitelistAsync(config);
    }

    private sealed class FolderListItem
    {
        public string DisplayName { get; init; } = string.Empty;
        public string FolderPath { get; init; } = string.Empty;
    }
}
