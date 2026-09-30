namespace FocusManager.Core.Models;

public sealed record AllowedFolder(string DisplayName, string FolderPath, bool AllowSubfolders = false);
