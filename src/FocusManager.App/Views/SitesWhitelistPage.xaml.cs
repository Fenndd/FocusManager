using System.Collections.ObjectModel;
using FocusManager.App.Services;
using FocusManager.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class SitesWhitelistPage : Page
{
    private readonly IAgentClient _agentClient = new AgentClient();
    private readonly ObservableCollection<SiteListItem> _items = [];

    public SitesWhitelistPage()
    {
        InitializeComponent();
        AllowedSitesList.ItemsSource = _items;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var config = await _agentClient.GetWhitelistAsync();
            _items.Clear();

            foreach (var site in config.AllowedSites)
            {
                _items.Add(new SiteListItem
                {
                    DisplayName = site.DisplayName,
                    HostPattern = site.HostPattern
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

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var hostPattern = NormalizeHostPattern(SiteHostInput.Text);

        if (string.IsNullOrWhiteSpace(hostPattern))
        {
            StatusText.Text = "Status: enter a valid host pattern (example: wikipedia.org or *.wikipedia.org).";
            return;
        }

        if (_items.Any(x => string.Equals(x.HostPattern, hostPattern, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText.Text = "Status: this host pattern is already in the whitelist.";
            return;
        }

        var displayName = SiteDisplayNameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = hostPattern;
        }

        _items.Add(new SiteListItem
        {
            DisplayName = displayName,
            HostPattern = hostPattern
        });

        try
        {
            await SaveSitesToAgentAsync();
            SiteDisplayNameInput.Text = string.Empty;
            SiteHostInput.Text = string.Empty;
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (AllowedSitesList.SelectedItem is not SiteListItem selected)
        {
            StatusText.Text = "Status: select an item to remove.";
            return;
        }

        _items.Remove(selected);

        try
        {
            await SaveSitesToAgentAsync();
            StatusText.Text = "Status: saved";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Status: failed to save ({ex.Message})";
        }
    }

    private async Task SaveSitesToAgentAsync()
    {
        var config = await _agentClient.GetWhitelistAsync();
        config.AllowedSites = _items
            .Select(x => new AllowedSite(x.DisplayName, x.HostPattern))
            .ToList();

        await _agentClient.SaveWhitelistAsync(config);
    }

    private static string NormalizeHostPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var value = input.Trim().Trim('/');

        if (value.Contains(' '))
        {
            return string.Empty;
        }

        if (string.Equals(value, "*", StringComparison.Ordinal))
        {
            return "*";
        }

        if (value.StartsWith("*.", StringComparison.Ordinal))
        {
            var suffix = NormalizeHost(value[2..]);
            return string.IsNullOrWhiteSpace(suffix)
                ? string.Empty
                : $"*.{suffix}";
        }

        return NormalizeHost(value);
    }

    private static string NormalizeHost(string value)
    {
        var candidate = value.Trim();

        if (candidate.Contains("*", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        if (candidate.Contains("://", StringComparison.Ordinal))
        {
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteUri))
            {
                return string.Empty;
            }

            return absoluteUri.Host.ToLowerInvariant();
        }

        if (candidate.Contains('/'))
        {
            candidate = candidate.Split('/')[0];
        }

        if (candidate.Contains(':'))
        {
            if (!Uri.TryCreate($"https://{candidate}", UriKind.Absolute, out var uriWithPort))
            {
                return string.Empty;
            }

            candidate = uriWithPort.Host;
        }

        if (Uri.CheckHostName(candidate) == UriHostNameType.Unknown)
        {
            return string.Empty;
        }

        return candidate.ToLowerInvariant();
    }

    private sealed class SiteListItem
    {
        public string DisplayName { get; init; } = string.Empty;
        public string HostPattern { get; init; } = string.Empty;
    }
}
