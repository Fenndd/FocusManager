using System.Text.Json;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Platform.Windows;
using Xunit;

namespace FocusManager.Platform.Windows.Tests;

/// <summary>
/// Minimal consumer of <c>test-vectors/windows-path-compat.json</c> (T054; the complete loader is T022). Runs every
/// case through <see cref="WindowsHostPathRules"/> with a synthetic host built from the case's <c>hostContext</c>
/// (current drive C: and drive roots when absent).
/// </summary>
public sealed class WindowsPathCompatVectorTests
{
    private static JsonElement Cases()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "test-vectors", "windows-path-compat.json");
        using var stream = File.OpenRead(path);
        return JsonDocument.Parse(stream).RootElement.GetProperty("cases").Clone();
    }

    public static TheoryData<string> CaseIds()
    {
        var ids = new TheoryData<string>();
        foreach (var vector in Cases().EnumerateArray())
        {
            ids.Add(vector.GetProperty("id").GetString()!);
        }

        return ids;
    }

    [Theory]
    [MemberData(nameof(CaseIds))]
    public void Vector_GivesTheExpectedDecision(string id)
    {
        var vector = Cases().EnumerateArray().Single(x => x.GetProperty("id").GetString() == id);
        Assert.Equal("windows", vector.GetProperty("pathStyle").GetString());

        var currentDrive = "C:";
        var directories = new List<(string Drive, string Directory)>();
        if (vector.TryGetProperty("hostContext", out var context))
        {
            currentDrive = context.GetProperty("currentDrive").GetString()!;
            foreach (var entry in context.GetProperty("currentDirectories").EnumerateObject())
            {
                directories.Add((entry.Name, entry.Value.GetString()!));
            }
        }

        var allowed = vector.GetProperty("allowed").EnumerateArray().ToList();
        var isFolder = vector.GetProperty("kind").GetString() == "folder";
        var config = isFolder
            ? new WhitelistConfig { AllowedFolders = [.. allowed.Select(x => new AllowedFolder("Folder", x.GetProperty("path").GetString()!, x.GetProperty("includeSubfolders").GetBoolean()))] }
            : new WhitelistConfig { AllowedApps = [.. allowed.Select(x => new AllowedApp("App", x.GetProperty("path").GetString()!))] };
        var input = vector.GetProperty("input").GetString()!;
        var sut = new RuleEvaluator(new WindowsHostPathRules(PathRules.Windows, WindowsHostPathRulesTests.SyntheticHost(currentDrive, [.. directories])));

        var decision = isFolder ? sut.EvaluateFolderOpen(input, config) : sut.EvaluateApplicationStart(input, config);

        Assert.Equal(vector.GetProperty("expected").GetString() == "allow", decision.IsAllowed);
    }
}
