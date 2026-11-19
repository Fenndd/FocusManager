using FocusManager.Core.Abstractions;
using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

/// <summary>
/// Describes one path style for parameterized tests. Paths are built from literal characters,
/// never from <c>System.IO.Path</c>, so both styles are exercised identically on any host OS.
/// </summary>
public sealed class PathStyleFixture
{
    public const string WindowsName = "windows";
    public const string UnixName = "unix";

    private PathStyleFixture(string name, IPathRules rules, string root, char separator)
    {
        Name = name;
        Rules = rules;
        Root = root;
        Separator = separator;
    }

    public string Name { get; }

    public IPathRules Rules { get; }

    /// <summary>Absolute root of the style, e.g. <c>C:\</c> or <c>/</c>.</summary>
    public string Root { get; }

    public char Separator { get; }

    public bool IsWindows => Name == WindowsName;

    public static TheoryData<string> Names => new() { WindowsName, UnixName };

    public static PathStyleFixture For(string name) => name switch
    {
        WindowsName => new PathStyleFixture(name, PathRules.Windows, @"C:\", '\\'),
        UnixName => new PathStyleFixture(name, PathRules.Unix, "/", '/'),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown path style."),
    };

    public RuleEvaluator CreateEvaluator() => new(Rules);

    /// <summary>Builds an absolute path in this style from the given segments.</summary>
    public string AbsolutePath(params string[] segments) => Root + string.Join(Separator, segments);

    public override string ToString() => Name;
}
