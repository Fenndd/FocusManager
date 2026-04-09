using System.Collections.ObjectModel;

namespace FocusManager.App.ViewModels;

public sealed class SitesWhitelistViewModel
{
    public ObservableCollection<string> AllowedHosts { get; } = [];
}
