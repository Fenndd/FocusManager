using System.Text.Json;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

/// <summary>
/// Minimal consumer of <c>test-vectors/windows-path-compat.json</c> (T054; the complete loader is T022).
/// Runs the Windows-style cases that need no host context through the pure <see cref="PathRules.Windows"/>.
/// </summary>
public sealed class WindowsPathCompatVectorTests
{
    private static JsonElement Cases()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "test-vectors", "windows-path-compat.json");
        using var stream = File.OpenRead(path);
        return JsonDocument.Parse(stream).RootElement.GetProperty("cases").Clone();
    }

    public static TheoryData<string> PureCaseIds()
    {
        var ids = new TheoryData<string>();
        foreach (var vector in Cases().EnumerateArray().Where(x => !x.TryGetProperty("hostContext", out _)))
        {
            ids.Add(vector.GetProperty("id").GetString()!);
        }

        return ids;
    }

    [Theory]
    [MemberData(nameof(PureCaseIds))]
    public void Vector_GivesTheExpectedDecision(string id)
    {
        var vector = Cases().EnumerateArray().Single(x => x.GetProperty("id").GetString() == id);
        Assert.Equal("windows", vector.GetProperty("pathStyle").GetString());

        var allowed = vector.GetProperty("allowed").EnumerateArray().ToList();
        var config = vector.GetProperty("kind").GetString() == "folder"
            ? new WhitelistConfig { AllowedFolders = [.. allowed.Select(x => new AllowedFolder("Folder", x.GetProperty("path").GetString()!, x.GetProperty("includeSubfolders").GetBoolean()))] }
            : new WhitelistConfig { AllowedApps = [.. allowed.Select(x => new AllowedApp("App", x.GetProperty("path").GetString()!))] };
        var input = vector.GetProperty("input").GetString()!;
        var sut = new RuleEvaluator(PathRules.Windows);

        var decision = vector.GetProperty("kind").GetString() == "folder"
            ? sut.EvaluateFolderOpen(input, config)
            : sut.EvaluateApplicationStart(input, config);

        Assert.Equal(vector.GetProperty("expected").GetString() == "allow", decision.IsAllowed);
    }

    [Fact]
    public void Vectors_IncludeCasesWithoutHostContext()
    {
        Assert.NotEmpty(PureCaseIds());
    }
}
