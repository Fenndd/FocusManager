using System.Text.RegularExpressions;
using FocusManager.Agent.Composition;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Platform.Windows;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FocusManager.Agent.Tests;

/// <summary>
/// T054: the agent must evaluate rules with the Windows host adapter, not with the pure <see cref="PathRules.Windows"/>.
/// Registering the pure rules would silently bring back the "D:" versus "D:\" defect while every other test stays green,
/// so the production composition is covered here: the registration extension that <c>Program.cs</c> calls is resolved
/// through a real container, and <c>Program.cs</c> itself is checked to use that extension.
/// </summary>
public sealed class PathRulesCompositionTests
{
    private static ServiceProvider BuildProvider() =>
        new ServiceCollection().AddWindowsPathRules().BuildServiceProvider();

    [Fact]
    public void AddWindowsPathRules_RegistersTheHostAdapter_NotThePureRules()
    {
        using var provider = BuildProvider();

        var rules = provider.GetRequiredService<IPathRules>();

        Assert.IsType<WindowsHostPathRules>(rules);
        Assert.NotSame(PathRules.Windows, rules);
    }

    [Fact]
    public void RegisteredEvaluator_UsesTheRegisteredRules_AndKeepsADriveRootStoredWithoutSeparatorAllowed()
    {
        using var provider = BuildProvider();
        var evaluator = provider.GetRequiredService<RuleEvaluator>();

        // The UI persists a selected "D:\" as "D:" (T054); before T001 the evaluator resolved it with Path.GetFullPath,
        // which is the current directory of drive D: (its root unless the process changed it). Use the host's own answer.
        var driveCurrentDirectory = Path.GetFullPath("D:");
        var config = new WhitelistConfig { AllowedFolders = [new AllowedFolder("D", "D:", AllowSubfolders: false)] };

        Assert.True(evaluator.EvaluateFolderOpen(driveCurrentDirectory, config).IsAllowed);
        Assert.False(evaluator.EvaluateFolderOpen(@"E:\Other", config).IsAllowed);
    }

    [Fact]
    public void ProgramCs_RegistersPathRulesOnlyThroughTheHostAdapterExtension()
    {
        var program = File.ReadAllText(FindProgramCs());

        // The top-level statements must call the extension...
        Assert.Matches(@"(?m)^\s*builder\.Services\.AddWindowsPathRules\(\)\s*;", program);

        // ...and nothing in the file may register or use the pure rules instead.
        Assert.DoesNotContain("PathRules.Windows", program, StringComparison.Ordinal);
        foreach (Match registration in Regex.Matches(program, @"AddSingleton<IPathRules>\([^;]*;"))
        {
            Assert.Contains("new WindowsHostPathRules()", registration.Value, StringComparison.Ordinal);
        }
    }

    private static string FindProgramCs()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Windows", "FocusManager.Agent", "Program.cs");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("src/Windows/FocusManager.Agent/Program.cs was not found above " + AppContext.BaseDirectory);
    }
}
