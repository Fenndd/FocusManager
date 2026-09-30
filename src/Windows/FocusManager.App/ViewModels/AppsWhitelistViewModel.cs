using System.Collections.ObjectModel;

namespace FocusManager.App.ViewModels;

public sealed class AppsWhitelistViewModel
{
    public ObservableCollection<string> AllowedExecutablePaths { get; } = [];
}
