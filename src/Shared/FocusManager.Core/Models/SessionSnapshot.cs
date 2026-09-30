namespace FocusManager.Core.Models;

public sealed class SessionSnapshot
{
    public HashSet<int> ExistingProcessIds { get; init; } = [];
    public DateTimeOffset CapturedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
