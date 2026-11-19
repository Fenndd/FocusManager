using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Platform.Windows;
using Xunit;

namespace FocusManager.Platform.Windows.Tests;

/// <summary>
/// T054 regression tests for the Windows compatibility fixes that need host context. Expected results are the
/// pre-T001 evaluator results (<c>git show HEAD:src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs</c>, which called
/// <c>Path.GetFullPath</c>) recorded on Windows 11 / .NET SDK 8.0.419. Context used for relative-path comparison:
/// current drive = the drive of the test process working directory; the per-drive current directory of any other
/// drive is its root (no <c>=D:</c> style environment variable), which the premise assertions below check.
/// </summary>
public sealed class WindowsHostPathRulesTests
{
    private static WhitelistConfig Folder(string path, bool subfolders) =>
        new() { AllowedFolders = [new AllowedFolder("Folder", path, subfolders)] };

    private static RuleEvaluator Host(Func<string, string>? resolve = null) =>
        new(resolve is null
            ? new WindowsHostPathRules()
            : new WindowsHostPathRules(PathRules.Windows, resolve));

    /// <summary>
    /// Mirrors <c>FoldersWhitelistPage.NormalizeFolderPath</c> (src/Windows/FocusManager.App/Views/FoldersWhitelistPage.xaml.cs);
    /// the WinUI project cannot be referenced from a test project. This is the value the UI persists for a selected folder.
    /// </summary>
    private static string UiNormalizeFolderPath(string path)
    {
        var cleaned = path.Trim().Trim('"');
        cleaned = Path.GetFullPath(cleaned);
        return cleaned.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>Synthetic Windows host: a current drive and per-drive current directories (no OS calls).</summary>
    internal static Func<string, string> SyntheticHost(string currentDrive, params (string Drive, string Directory)[] directories)
    {
        string DirectoryOf(string drive) =>
            directories.Where(x => string.Equals(x.Drive, drive, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Directory).FirstOrDefault() ?? drive + @"\";

        return path =>
        {
            if (path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':')
            {
                return DirectoryOf(path[..2]).TrimEnd('\\') + @"\" + path[2..];
            }

            if (path.StartsWith('\\'))
            {
                return currentDrive + path;
            }

            return DirectoryOf(currentDrive).TrimEnd('\\') + @"\" + path;
        };
    }

    private static string CurrentDrive() => Path.GetPathRoot(Directory.GetCurrentDirectory())![..2];

    // ---- Review difference 1: UI saves "D:" for a selected "D:\" ----------------------------------

    [Fact]
    public void UiPersistedDriveRoot_StillAllowsTheDriveRoot_AfterReload_RealGetFullPath()
    {
        // Premise (documented context): D: has no per-drive current directory other than its root.
        Assert.Equal(@"D:\", Path.GetFullPath("D:"));

        var persisted = UiNormalizeFolderPath(@"D:\");
        Assert.Equal("D:", persisted); // the UI boundary really stores the drive without a separator

        var sut = Host();

        Assert.True(sut.EvaluateFolderOpen(@"D:\", Folder(persisted, subfolders: false)).IsAllowed); // HEAD: allow
        Assert.True(sut.EvaluateFolderOpen(@"D:\Study", Folder(persisted, subfolders: true)).IsAllowed); // HEAD: allow
        Assert.False(sut.EvaluateFolderOpen(@"D:\Study", Folder(persisted, subfolders: false)).IsAllowed); // HEAD: deny
        Assert.False(sut.EvaluateFolderOpen(@"E:\", Folder(persisted, subfolders: true)).IsAllowed); // HEAD: deny
    }

    [Theory]
    [InlineData(@"D:\", "D:", false, true)]
    [InlineData(@"D:\Study", "D:", true, true)]
    [InlineData(@"D:\Study", "D:", false, false)]
    [InlineData(@"E:\", "D:", true, false)]
    public void DriveRootWithoutSeparator_Synthetic_RootCurrentDirectory(string candidate, string allowed, bool subfolders, bool expected)
    {
        var sut = Host(SyntheticHost("C:"));

        Assert.Equal(expected, sut.EvaluateFolderOpen(candidate, Folder(allowed, subfolders)).IsAllowed);
    }

    [Fact]
    public void DriveWithoutSeparator_ResolvesToThePerDriveCurrentDirectory_AsBefore()
    {
        // HEAD: Path.GetFullPath("D:") is the current directory of drive D:, not necessarily its root.
        var sut = Host(SyntheticHost("C:", ("D:", @"D:\Work")));

        Assert.True(sut.EvaluateFolderOpen(@"D:\Work", Folder("D:", false)).IsAllowed);
        Assert.False(sut.EvaluateFolderOpen(@"D:\", Folder("D:", false)).IsAllowed);
        Assert.True(sut.EvaluateFolderOpen(@"D:\Work\Math", Folder("D:", true)).IsAllowed);
        Assert.True(sut.EvaluateFolderOpen(@"D:\Work\Study", Folder("D:Study", false)).IsAllowed);
    }

    [Fact]
    public void CurrentDriveWithoutSeparator_ResolvesToTheCurrentDirectory_RealGetFullPath()
    {
        var cwd = Directory.GetCurrentDirectory();
        var sut = Host();

        Assert.True(sut.EvaluateFolderOpen(cwd, Folder(CurrentDrive(), false)).IsAllowed); // HEAD: allow (GetFullPath("C:") == cwd)
    }

    // ---- Review difference 2: root-relative "\Study" ---------------------------------------------

    [Fact]
    public void RootRelativePath_ResolvesAgainstTheCurrentDrive_RealGetFullPath()
    {
        var drive = CurrentDrive();
        var sut = Host();

        Assert.True(sut.EvaluateFolderOpen(drive + @"\Study", Folder(@"\Study", false)).IsAllowed); // HEAD: allow
        Assert.False(sut.EvaluateFolderOpen(drive + @"\Study\Math", Folder(@"\Study", false)).IsAllowed); // HEAD: deny
        Assert.True(sut.EvaluateFolderOpen(drive + @"\Study\Math", Folder(@"\Study", true)).IsAllowed); // HEAD: allow
    }

    [Theory]
    [InlineData(@"C:\Study", @"\Study", false, true)]
    [InlineData(@"c:\study", @"\Study", false, true)]
    [InlineData(@"D:\Study", @"\Study", false, false)]
    [InlineData(@"C:\Study\Math", @"\Study", true, true)]
    [InlineData(@"C:\Study", @"\Study\..\Other", false, false)]
    public void RootRelativePath_Synthetic_CurrentDriveC(string candidate, string allowed, bool subfolders, bool expected)
    {
        var sut = Host(SyntheticHost("C:"));

        Assert.Equal(expected, sut.EvaluateFolderOpen(candidate, Folder(allowed, subfolders)).IsAllowed);
    }

    [Fact]
    public void PlainRelativePath_ResolvesAgainstTheCurrentDirectory_AsBefore()
    {
        var sut = Host(SyntheticHost("C:", ("C:", @"C:\Users\Me")));

        Assert.True(sut.EvaluateFolderOpen(@"C:\Users\Me\Study", Folder("Study", false)).IsAllowed);
        Assert.False(sut.EvaluateFolderOpen(@"C:\Study", Folder("Study", false)).IsAllowed);
    }

    // T063/F1: the third leading separator is an empty UNC server slot, not an ordinary server.
    [Theory]
    [InlineData(@"\\\srv\share\x", @"\\srv\share", false)]
    [InlineData(@"\\srv\share\x", @"\\\srv\share", false)]
    [InlineData(@"\\\\srv\share\x", @"\\srv\share", false)]
    [InlineData(@"\\srv\share\x", @"\\\\srv\share", false)]
    [InlineData("///srv/share/x", "//srv/share", false)]
    [InlineData("//srv/share/x", "///srv/share", false)]
    [InlineData(@"\\srv\share\x", @"\\srv\share", true)]
    [InlineData(@"\\\srv\share\x", @"\\\srv\share", true)]
    public void ExtraLeadingUncSeparators_PreservePreT001FolderDecision(string candidate, string allowed, bool expected)
    {
        Assert.Equal(expected, Host().EvaluateFolderOpen(candidate, Folder(allowed, true)).IsAllowed);
    }

    // ---- Adapter behavior ------------------------------------------------------------------------

    [Theory]
    [InlineData(@"C:\Study")]
    [InlineData("C:/Study/Math")]
    [InlineData(@"\\server\share\dir")]
    [InlineData("//server/share/dir")]
    public void FullyQualifiedPaths_AreNotResolvedAgainstHostState(string path)
    {
        var sut = new WindowsHostPathRules(PathRules.Windows, _ => throw new InvalidOperationException("host state was consulted"));

        Assert.Equal(PathRules.Windows.Normalize(path), sut.Normalize(path));
    }

    [Fact]
    public void ResolverFailure_KeepsTheBestEffortNormalizedPath_AsBefore()
    {
        var sut = new WindowsHostPathRules(PathRules.Windows, _ => throw new NotSupportedException());

        Assert.Equal(@"Study\Math", sut.Normalize(@"  ""Study\.\Math""  "));
        Assert.Equal(string.Empty, sut.Normalize("   "));
    }

    [Fact]
    public void ColonInFinalComponent_IsNotAFilenameBoundary_ThroughTheAdapter()
    {
        var sut = Host();
        var config = new WhitelistConfig { AllowedApps = [new AllowedApp("Code", @"C:\Apps\Code.exe")] };

        Assert.False(sut.EvaluateApplicationStart(@"C:\Apps\evil.exe:Code.exe", config).IsAllowed); // HEAD: deny
        Assert.True(sut.EvaluateApplicationStart(@"C:\Apps\Code.exe", config).IsAllowed);
    }

    [Fact]
    public void ShortNameInFullyQualifiedPath_IsExpandedByTheHost_AsBefore()
    {
        // HEAD: Path.GetFullPath expanded 8.3 short names ("~") of existing directories, so an allowed long
        // path matched its short form. Without the fix, only the purely lexical rules ran for fully qualified paths.
        var sut = Host();
        var expandedByHost = !Path.GetFullPath(@"C:\PROGRA~1\FocusManagerT054").Contains('~');

        var decision = sut.EvaluateFolderOpen(@"C:\Program Files\FocusManagerT054\Math", Folder(@"C:\PROGRA~1\FocusManagerT054", subfolders: true));
        var reverse = sut.EvaluateFolderOpen(@"C:\PROGRA~1\FocusManagerT054\Math", Folder(@"C:\Program Files\FocusManagerT054", subfolders: true));

        Assert.Equal(expandedByHost, decision.IsAllowed); // HEAD result on the host (8.3 names can be disabled per volume)
        Assert.Equal(expandedByHost, reverse.IsAllowed);
        Assert.Equal(PreT001Normalize(@"C:\PROGRA~1\x"), new WindowsHostPathRules().Normalize(@"C:\PROGRA~1\x").TrimEnd('\\'));
    }

    [Fact]
    public void ShortNameInFullyQualifiedPath_GoesThroughTheHostResolver()
    {
        var calls = new List<string>();
        var sut = new WindowsHostPathRules(PathRules.Windows, path =>
        {
            calls.Add(path);
            return path.Replace("PROGRA~1", "Program Files", StringComparison.Ordinal);
        });

        Assert.Equal(@"C:\Program Files\App", sut.Normalize(@"C:\PROGRA~1\App"));
        Assert.Equal([@"C:\PROGRA~1\App"], calls);
    }

    // ---- Parity with the pre-T001 normalization on fully qualified paths --------------------------

    /// <summary>The pre-T001 <c>NormalizePath</c>: <c>Path.GetFullPath</c> then trailing separators trimmed.</summary>
    private static string PreT001Normalize(string path)
    {
        var cleaned = path.Trim().Trim('"');
        try
        {
            cleaned = Path.GetFullPath(cleaned);
        }
        catch
        {
            // Pre-T001 kept the raw value.
        }

        return cleaned.TrimEnd('\\', '/');
    }

    /// <summary>
    /// Fully qualified prefixes: drive (both separators), UNC, device (<c>\\.\</c>, <c>\\.\C:\</c>, <c>//./</c>), the
    /// forward-slash extended form (<c>//?/</c>, which .NET normalizes) and the literal verbatim forms
    /// (<c>\\?\</c>, <c>\??\</c>) that .NET leaves untouched.
    /// </summary>
    public static TheoryData<string> Prefixes => new()
    {
        @"C:\", @"c:/", @"\\srv\share\",
        @"\\.\", @"\\.\C:\", @"//./C:/", @"//?/C:/",
        @"\\?\C:\", @"\??\C:\",
    };

    private static readonly string[] ParityTokens = ["a", "b", ".", "..", " ", "\\", "/", "a.", ":"];

    private static List<string> ParityMismatches(string prefix, string[] tokens, Func<string, string> normalize, out int checkedCount, int maxDepth = 4, Func<string, bool>? skipTail = null)
    {
        var mismatches = new List<string>();
        var count = 0;

        void Visit(string tail, int depth)
        {
            if (tail.Length > 0 && !(skipTail?.Invoke(tail) ?? false))
            {
                var input = prefix + tail;
                count++;
                var expected = PreT001Normalize(input);
                var actual = normalize(input).TrimEnd('\\', '/');
                if (!string.Equals(expected, actual, StringComparison.Ordinal))
                {
                    mismatches.Add($"[{input}] expected [{expected}] actual [{actual}]");
                }
            }

            if (depth == maxDepth)
            {
                return;
            }

            foreach (var token in tokens)
            {
                Visit(tail + token, depth + 1);
            }
        }

        Visit(string.Empty, 0);
        checkedCount = count;
        return mismatches;
    }

    /// <summary>
    /// The purely lexical rules reproduce <c>Path.GetFullPath</c> for every fully qualified path built from the
    /// tokens (dot segments, dot and space runs, mixed separators, colons) without any host call.
    /// Documented exclusion: 8.3 short names ("~") need the file system and are covered by the adapter test below.
    /// </summary>
    [Theory]
    [MemberData(nameof(Prefixes))]
    public void PureRules_Normalize_MatchPreT001GetFullPath_ForGeneratedFullyQualifiedPaths(string prefix)
    {
        var mismatches = ParityMismatches(prefix, ParityTokens, PathRules.Windows.Normalize, out var count);

        Assert.True(count > 6000);
        Assert.True(mismatches.Count == 0, $"{mismatches.Count} mismatches, first: {string.Join(" | ", mismatches.Take(10))}");
    }

    /// <summary>
    /// Non-verbatim device prefixes (<c>\\.\</c> and <c>//?/</c> forms). When such a path contains "~", .NET's own
    /// short-name expansion garbles the result (for example <c>\\.\C:\..\a~</c> becomes <c>\\.\\~</c> on .NET 8.0.25), so
    /// bit-for-bit parity with it is not a goal there; these prefixes are excluded from the "~" run below and covered
    /// by <see cref="DevicePathWithShortNameCharacter_IsExcludedFromParity_ButStaysDeviceRooted"/>.
    /// </summary>
    private static bool IsNonVerbatimDevicePrefix(string prefix) =>
        prefix.Replace('/', '\\').StartsWith(@"\\.\", StringComparison.Ordinal)
        || (prefix.StartsWith("//?/", StringComparison.Ordinal));

    /// <summary>The production adapter (what Program.cs registers) also matches when the tokens include "~".</summary>
    [Theory]
    [MemberData(nameof(Prefixes))]
    public void Adapter_Normalize_MatchesPreT001GetFullPath_ForGeneratedFullyQualifiedPaths_IncludingShortNameCharacter(string prefix)
    {
        var sut = new WindowsHostPathRules();
        string[] tokens = IsNonVerbatimDevicePrefix(prefix) ? ParityTokens : [.. ParityTokens, "~"];

        var mismatches = ParityMismatches(prefix, tokens, sut.Normalize, out var count);

        Assert.True(count > 6000);
        Assert.True(mismatches.Count == 0, $"{mismatches.Count} mismatches, first: {string.Join(" | ", mismatches.Take(10))}");
    }

    /// <summary>
    /// UNC prefixes that leave the server and share slots to the generator: "." and ".." there are literal root
    /// names (not dot segments), a trailing dot or space of the final component is trimmed even when that component
    /// is the share or the server, and doubled or mixed separators collapse. Tokens are kept to the relevant ones so
    /// that depth 6 reaches paths such as <c>\\..\.\x\..\y</c>. 8.3 expansion ("~") is covered by the other generator.
    /// </summary>
    public static TheoryData<string> UncSlotPrefixes => new()
    {
        @"\\", "//", @"\\srv\", @"\\a\b\",
    };

    private static readonly string[] UncSlotTokens = ["a", ".", "..", " ", "\\", "/", "a."];

    [Theory]
    [MemberData(nameof(UncSlotPrefixes))]
    public void PureRules_Normalize_MatchPreT001GetFullPath_WithDotsAndSpacesInUncServerAndShareSlots(string prefix)
    {
        var mismatches = ParityMismatches(prefix, UncSlotTokens, PathRules.Windows.Normalize, out var count, maxDepth: 6);

        Assert.True(count > 50000);
        Assert.True(mismatches.Count == 0, $"{mismatches.Count} mismatches, first: {string.Join(" | ", mismatches.Take(10))}");
    }

    [Theory]
    [MemberData(nameof(UncSlotPrefixes))]
    public void Adapter_Normalize_MatchPreT001GetFullPath_WithDotsAndSpacesInUncServerAndShareSlots(string prefix)
    {
        var mismatches = ParityMismatches(prefix, UncSlotTokens, new WindowsHostPathRules().Normalize, out var count, maxDepth: 6);

        Assert.True(count > 50000);
        Assert.True(mismatches.Count == 0, $"{mismatches.Count} mismatches, first: {string.Join(" | ", mismatches.Take(10))}");
    }

    // The review's examples: with an allowed share and subfolders, a path whose server or share slot is "." / ".."
    // is a different (literal) root and must not be allowed (HEAD: deny).
    [Theory]
    [InlineData(@"\\srv\..\share\x", false)]
    [InlineData(@"\\srv\.\share\x", false)]
    [InlineData(@"\\..\srv\share\x", false)]
    [InlineData(@"\\.\srv\share\x", false)]
    [InlineData(@"\\srv\share\x", true)] // control
    [InlineData(@"\\srv\share\..\x", true)] // ".." after the share cannot leave it
    [InlineData(@"\\srv\share.\x", false)] // inner trailing dot of the share is kept: another share
    public void UncFolder_DotSegmentsInServerOrShareSlot_AreLiteral_ThroughTheAdapter(string candidate, bool expectedAllowed)
    {
        var sut = Host();

        Assert.Equal(expectedAllowed, sut.EvaluateFolderOpen(candidate, Folder(@"\\srv\share", subfolders: true)).IsAllowed);
    }

    [Theory]
    [InlineData(@"\\srv\share.", @"\\srv\share\x")] // Win32 trims the trailing dot of the final component, share included
    [InlineData(@"\\srv\sharea..", @"\\srv\sharea\x")]
    [InlineData(@"\\srv\share \", @"\\srv\share \x")] // a trailing separator protects the share from trimming
    public void UncShareTrailingDots_FollowWin32Trimming_ThroughTheAdapter(string allowed, string candidate)
    {
        var sut = Host();
        var expected = PreT001Normalize(candidate).StartsWith(PreT001Normalize(allowed) + @"\", StringComparison.OrdinalIgnoreCase);

        Assert.Equal(expected, sut.EvaluateFolderOpen(candidate, Folder(allowed, subfolders: true)).IsAllowed);
    }

    [Theory]
    [InlineData(@"\\.\C:\..\a~")]
    [InlineData(@"\\.\aaa~")]
    [InlineData(@"//./C:/..\b~")]
    [InlineData(@"//?/C:/..\~a")]
    public void DevicePathWithShortNameCharacter_IsExcludedFromParity_ButStaysDeviceRooted(string input)
    {
        // Documented exclusion (see IsNonVerbatimDevicePrefix): the adapter does not throw and keeps the device root.
        var normalized = new WindowsHostPathRules().Normalize(input);

        Assert.Matches(@"^\\\\[.?]\\", normalized);
    }
}
