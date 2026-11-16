using System.Collections.ObjectModel;

namespace FocusManager.App.ViewModels;

public sealed class FoldersWhitelistViewModel
{
    public ObservableCollection<string> AllowedFolderPaths { get; } = [];
}
