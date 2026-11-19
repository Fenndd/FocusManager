using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

/// <summary>
/// T054 regression tests for the pure (host-independent) part of the Windows compatibility fixes found
/// by the 2026-11-18 review. Every expected value is the result of the pre-T001 evaluator
/// (<c>git show HEAD:src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs</c>, which used
/// <c>Path.GetFullPath</c> and <c>Path.GetFileName</c> on Windows/.NET 8); none is derived from the new code.
/// The context-dependent cases (<c>D:</c>, <c>\Study</c>) live in the Platform.Windows tests, because they need
/// Windows host state that shared Core must not read.
/// </summary>
public sealed class WindowsCompatibilityRegressionTests
{
    private readonly RuleEvaluator _sut = new(PathRules.Windows);

    private static WhitelistConfig Folder(string path, bool subfolders) =>
        new() { AllowedFolders = [new AllowedFolder("Folder", path, subfolders)] };

    private static WhitelistConfig App(string path) =>
        new() { AllowedApps = [new AllowedApp("App", path)] };

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
        Assert.Equal(expected, _sut.EvaluateFolderOpen(candidate, Folder(allowed, true)).IsAllowed);
    }

    [Theory]
    [InlineData(@"\\\srv\share\x", @"\\\srv\share\x")]
    [InlineData(@"\\\\srv\share\x", @"\\\srv\share\x")]
    [InlineData(@"\\\\\srv\share\x", @"\\\srv\share\x")]
    [InlineData(@"\\\srv\..\x", @"\\\srv\x")]
    [InlineData(@"\\\\..\x", @"\\\x")]
    [InlineData(@"\\\a.\..", @"\\\a")]
    [InlineData(@"\\\\a.\..", "")]
    [InlineData(@"\\\..", "")]
    [InlineData(@"\\\", "")]
    [InlineData(@"\\\\", "")]
    [InlineData("///srv/share/x", @"\\\srv\share\x")]
    [InlineData(@"\\srv\share\x", @"\\srv\share\x")]
    public void ExtraLeadingUncSeparators_NormalizeLikePreT001(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\', '/'));
    }

    // Review difference: allowed "C:\Study." (HEAD allowed C:\Study) and the unchanged child-folder control.
    [Theory]
    [InlineData(@"C:\Study", @"C:\Study.", false, true)]
    [InlineData(@"C:\Study", @"C:\Study..", false, true)]
    [InlineData(@"C:\Study", @"C:\Study. ", false, true)]
    [InlineData(@"C:\Study.", @"C:\Study", false, true)]
    [InlineData(@"C:\Study\Math", @"C:\Study.", true, true)]
    [InlineData(@"C:\Study.\Math", @"C:\Study", true, true)]
    [InlineData(@"C:\Study\Math", @"C:\Study", true, true)] // control: unchanged by the review
    [InlineData(@"C:\Study\Math", @"C:\Study", false, false)] // control: subfolders not allowed
    [InlineData(@"C:\StudyOther", @"C:\Study.", true, false)]
    public void EvaluateFolderOpen_MatchesPreRefactorResult(string candidate, string allowed, bool subfolders, bool expectedAllowed)
    {
        var decision = _sut.EvaluateFolderOpen(candidate, Folder(allowed, subfolders));

        Assert.Equal(expectedAllowed, decision.IsAllowed);
    }

    // Review difference: "C:\Apps\evil.exe:Code.exe" must not match allowed "C:\Apps\Code.exe" by file name (HEAD: deny).
    [Fact]
    public void EvaluateApplicationStart_Denies_ColonInFinalComponent_AlternateDataStreamStyle()
    {
        var decision = _sut.EvaluateApplicationStart(@"C:\Apps\evil.exe:Code.exe", App(@"C:\Apps\Code.exe"));

        Assert.False(decision.IsAllowed);
    }

    [Theory]
    [InlineData(@"C:\Apps\Code.exe", true)]
    [InlineData(@"C:\Apps\Code.exe.", true)]
    [InlineData(@"C:\Apps\Code.exe ", true)]
    [InlineData(@"c:/apps/CODE.EXE", true)]
    [InlineData(@"Code.exe", true)] // retained file-name fallback (T012 owns changing it)
    [InlineData(@"C:Code.exe", true)] // drive prefix colon is still a boundary (Path.GetFileName on Windows)
    [InlineData(@"C:\Apps\Code.exe:Zone.Identifier", false)]
    [InlineData(@"C:\Apps\evil.exe:Code.exe", false)]
    [InlineData(@"evil.exe:Code.exe", false)]
    [InlineData(@"C:\Other\evil.exe:Code.exe", false)]
    public void EvaluateApplicationStart_MatchesPreRefactorResult(string candidate, bool expectedAllowed)
    {
        var decision = _sut.EvaluateApplicationStart(candidate, App(@"C:\Apps\Code.exe"));

        Assert.Equal(expectedAllowed, decision.IsAllowed);
    }

    [Theory]
    [InlineData(@"C:\Apps\evil.exe:Code.exe", "evil.exe:Code.exe")]
    [InlineData(@"evil.exe:Code.exe", "evil.exe:Code.exe")]
    [InlineData(@"C:\Apps\Code.exe:Zone.Identifier", "Code.exe:Zone.Identifier")]
    [InlineData(@"C:Code.exe", "Code.exe")]
    [InlineData(@"C:", "")]
    [InlineData(@"C:\", "")]
    public void Windows_GetFileName_TreatsColonAsBoundaryOnlyInDrivePrefix(string normalized, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.GetFileName(normalized));
    }

    // Values recorded from Path.GetFullPath(...).TrimEnd('\\', '/') on Windows 11 / .NET SDK 8.0.419.
    [Theory]
    [InlineData(@"C:\Study.", @"C:\Study")]
    [InlineData(@"C:\Study..", @"C:\Study")]
    [InlineData(@"C:\Study. ", @"C:\Study")]
    [InlineData(@"C:\Study.\Math", @"C:\Study\Math")] // a single trailing dot is dropped from inner segments
    [InlineData(@"C:\Study ", @"C:\Study")]
    [InlineData(@"C:\Study \Math", @"C:\Study \Math")] // inner trailing space is kept
    [InlineData(@"C:\a..\b", @"C:\a..\b")] // inner trailing double dot is kept
    [InlineData(@"C:\a\...\b", @"C:\a\...\b")] // a run of three or more dots is an ordinary name when inner
    [InlineData(@"C:\p\...", @"C:\p")] // ...but is trimmed away as the final component
    [InlineData(@"C:\Study.\", @"C:\Study")]
    [InlineData(@"C:\Study ..\", @"C:\Study ..")] // a trailing separator protects the final component from trimming
    [InlineData(@"C:\Apps\Code.exe.", @"C:\Apps\Code.exe")]
    [InlineData(@"C:\Apps\Code.exe ", @"C:\Apps\Code.exe")]
    [InlineData(@"C:\Apps\evil.exe:Code.exe", @"C:\Apps\evil.exe:Code.exe")]
    public void Windows_Normalize_MatchesPreRefactorTrailingDotAndSpaceHandling(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\'));
    }

    // ---- Review follow-up: device ("\\.\") and verbatim ("\\?\") paths ------------------------------
    // Values recorded from Path.GetFullPath(...).TrimEnd('\\', '/') on Windows 11 / .NET runtime 8.0.25 (SDK 8.0.419).

    // A device root ("\\.\" or "\\?\" written with forward slashes) is only a prefix; what follows is normalized like
    // a rooted path whose ".." cannot climb above the prefix. The "." / "?" is never a UNC server.
    [Theory]
    [InlineData(@"\\.\C:\x", @"\\.\C:\x")]
    [InlineData(@"\\.\C:\Study\..\Other", @"\\.\C:\Other")]
    [InlineData(@"\\.\C:\Study\..\..\Windows", @"\\.\Windows")]
    [InlineData(@"\\.\C:\..\..\..\x", @"\\.\x")]
    [InlineData(@"\\.\C:\..", @"\\.")]
    [InlineData(@"\\.\x\y", @"\\.\x\y")]
    [InlineData(@"\\.\C:", @"\\.\C:")]
    [InlineData(@"\\.\C:\a.", @"\\.\C:\a")]
    [InlineData(@"\\.\C:.\x", @"\\.\C:\x")]
    [InlineData(@"\\.\C:\a\\b", @"\\.\C:\a\b")]
    [InlineData(@"\\.\.\x", @"\\.\x")]
    [InlineData(@"\\.\pipe\x\..\y", @"\\.\pipe\y")]
    [InlineData(@"\\.\UNC\srv\sh\a\..\..\..\b", @"\\.\UNC\b")]
    [InlineData(@"\\.", @"\\.")]
    [InlineData(@"//./C:/x", @"\\.\C:\x")]
    [InlineData(@"//./C:/x/../../y", @"\\.\y")]
    [InlineData(@"//?/C:/x.", @"\\?\C:\x")] // forward slashes are not the verbatim prefix: normalized
    [InlineData(@"//?/C:/x/../y", @"\\?\C:\y")]
    public void Windows_Normalize_DevicePaths_MatchPreRefactor(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\'));
    }

    // Win32 and .NET do not normalize a path with the literal verbatim prefix "\\?\" (or "\??\"): dot segments,
    // doubled separators, forward slashes and trailing dots are kept; only trailing separators were trimmed.
    [Theory]
    [InlineData(@"\\?\C:\x.", @"\\?\C:\x.")]
    [InlineData(@"\\?\C:\x..", @"\\?\C:\x..")]
    [InlineData(@"\\?\C:\Study\..\Other", @"\\?\C:\Study\..\Other")]
    [InlineData(@"\\?\C:\Study\..\..\Windows", @"\\?\C:\Study\..\..\Windows")]
    [InlineData(@"\\?\C:\a\.\b", @"\\?\C:\a\.\b")]
    [InlineData(@"\\?\C:\a\\b", @"\\?\C:\a\\b")]
    [InlineData(@"\\?\C:\x/y", @"\\?\C:\x/y")]
    [InlineData(@"\\?\C:/x.", @"\\?\C:/x.")]
    [InlineData(@"\\?\C:\a\b\", @"\\?\C:\a\b")]
    [InlineData(@"\\?\UNC\srv\sh\a\..\b", @"\\?\UNC\srv\sh\a\..\b")]
    [InlineData(@"\\?\Volume{12345678-1234-1234-1234-123456789012}\x\..\y", @"\\?\Volume{12345678-1234-1234-1234-123456789012}\x\..\y")]
    [InlineData(@"\??\C:\a\..\b", @"\??\C:\a\..\b")]
    [InlineData(@"\??\C:\x.", @"\??\C:\x.")]
    public void Windows_Normalize_VerbatimPaths_AreNotNormalized(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\', '/'));
    }

    // ---- Second review (F1): "." and ".." in the UNC server or share slot --------------------------
    // Win32 and .NET take the first two segments of "\\server\share" as the root, literally: "." and ".." there are
    // names, not dot segments, and later ".." cannot remove them. Values recorded from Path.GetFullPath(...).TrimEnd('\\', '/')
    // on Windows 11 / .NET runtime 8.0.25 (SDK 8.0.419).
    [Theory]
    [InlineData(@"\\srv\..\share\x", @"\\srv\..\share\x")]
    [InlineData(@"\\srv\.\share\x", @"\\srv\.\share\x")]
    [InlineData(@"\\..\srv\share\x", @"\\..\srv\share\x")]
    [InlineData(@"\\.\srv\share\x", @"\\.\srv\share\x")] // device root, unchanged
    [InlineData(@"//srv/../share/x", @"\\srv\..\share\x")]
    [InlineData(@"\\srv\.\.\x", @"\\srv\.\x")] // share "." is literal, the second "." is a dot segment
    [InlineData(@"\\srv\..\..\share", @"\\srv\..\share")]
    [InlineData(@"\\..\..\x", @"\\..\..\x")]
    [InlineData(@"\\..\..\..\x", @"\\..\..\x")] // the third ".." cannot remove the root
    [InlineData(@"\\srv\share\..\x", @"\\srv\share\x")]
    [InlineData(@"\\srv\share\..\..\x", @"\\srv\share\x")]
    [InlineData(@"\\srv\a\b\..\..\..\..\c", @"\\srv\a\c")]
    [InlineData(@"\\srv\..\", @"\\srv\..")] // a trailing separator protects a "." / ".." share from trimming
    [InlineData(@"\\srv\.\", @"\\srv\.")]
    [InlineData(@"\\..\", @"\\..")]
    [InlineData(@"\\..\..\", @"\\..\..")]
    [InlineData(@"\\srv.\.\a", @"\\srv.\.\a")]
    [InlineData(@"\\srv.\a", @"\\srv.\a")]
    [InlineData(@"\\a.\..\b", @"\\a.\..\b")]
    // A doubled separator after the server makes the share empty: only the server is a root name then.
    [InlineData(@"\\srv\\share\x", @"\\srv\share\x")]
    [InlineData(@"\\srv\\aaa\..", @"\\srv")]
    [InlineData(@"\\srv\\..\x", @"\\srv\x")]
    [InlineData(@"\\aa.\\.", @"\\aa.")] // the server keeps its dot: the empty share is the final component
    [InlineData(@"\\aa.\\x\..", @"\\aa.")]
    [InlineData(@"\\ \\.", @"\\ ")]
    public void Windows_Normalize_UncServerAndShareSlots_AreLiteral(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\'));
    }

    // Win32 trims trailing dots and spaces of the final component of a path that does not end in a separator,
    // also when that component is the UNC share or server (the share or server becomes shorter, or disappears).
    [Theory]
    [InlineData(@"\\srv\share.", @"\\srv\share")]
    [InlineData(@"\\srv\sharea..", @"\\srv\sharea")]
    [InlineData(@"\\srv\share .", @"\\srv\share")]
    [InlineData(@"\\srv\share. .", @"\\srv\share")]
    [InlineData(@"\\srv\share...", @"\\srv\share")]
    [InlineData(@"\\srv\.", @"\\srv")] // the share "." is trimmed away
    [InlineData(@"\\srv\..", @"\\srv")]
    [InlineData(@"\\srv\...", @"\\srv")]
    [InlineData(@"\\srv\ .", @"\\srv")]
    [InlineData(@"\\srv.", @"\\srv")]
    [InlineData(@"\\srv..", @"\\srv")]
    [InlineData(@"\\..", "")] // the root alone: "\\" (the assertion trims trailing separators)
    [InlineData(@"\\a.\..", @"\\a.")]
    [InlineData(@"\\srv\share.\a\..", @"\\srv\share")]
    [InlineData(@"\\srv\.\a\..", @"\\srv")]
    [InlineData(@"\\srv\share.\", @"\\srv\share.")] // a trailing separator keeps the share as written
    [InlineData(@"\\srv\share.\x", @"\\srv\share.\x")] // an inner share keeps its dot
    [InlineData(@"\\srv\share..\x", @"\\srv\share..\x")]
    [InlineData(@"\\srv\share. \x", @"\\srv\share. \x")]
    public void Windows_Normalize_UncFinalComponentTrimming_IncludesServerAndShare(string input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input).TrimEnd('\\'));
    }

    // The review's example: with the share allowed (subfolders on), HEAD denied a path whose server/share slot is a dot name.
    [Theory]
    [InlineData(@"\\srv\share\x", true)] // control
    [InlineData(@"\\srv\..\share\x", false)]
    [InlineData(@"\\srv\.\share\x", false)]
    [InlineData(@"\\..\srv\share\x", false)]
    [InlineData(@"\\srv\share\..\x", true)]
    [InlineData(@"\\SRV\SHARE\x", true)]
    public void EvaluateFolderOpen_UncShare_DotNamesInRootSlotsDoNotCollapse(string candidate, bool expectedAllowed)
    {
        Assert.Equal(expectedAllowed, _sut.EvaluateFolderOpen(candidate, Folder(@"\\srv\share", true)).IsAllowed);
    }

    // Allowed share written with trailing dots: HEAD trimmed it, so the share and its children stay allowed.
    [Theory]
    [InlineData(@"\\srv\share", @"\\srv\share.", false, true)]
    [InlineData(@"\\srv\share\x", @"\\srv\share.", true, true)]
    [InlineData(@"\\srv\share\x", @"\\srv\sharea..", true, false)]
    [InlineData(@"\\srv\sharea\x", @"\\srv\sharea..", true, true)]
    [InlineData(@"\\srv\share.", @"\\srv\share", false, true)]
    public void EvaluateFolderOpen_UncShareTrailingDots_MatchPreRefactorResult(string candidate, string allowed, bool subfolders, bool expectedAllowed)
    {
        Assert.Equal(expectedAllowed, _sut.EvaluateFolderOpen(candidate, Folder(allowed, subfolders)).IsAllowed);
    }

    // Path.GetFileName skips the root: a UNC server/share, a device or verbatim volume and "\\?\UNC\server\share"
    // are never a file name. Values recorded from Path.GetFileName on .NET 8.0.25.
    [Theory]
    [InlineData(@"\\server\share", "")]
    [InlineData(@"\\server", "")]
    [InlineData(@"\\server\Code.exe", "")]
    [InlineData(@"\\server\share\a.exe", "a.exe")]
    [InlineData(@"\\.\Code.exe", "")]
    [InlineData(@"\\?\Code.exe", "")]
    [InlineData(@"\\?\C:", "")]
    [InlineData(@"\\?\C:\", "")]
    [InlineData(@"\\.\C:", "")]
    [InlineData(@"\\.\C:\", "")]
    [InlineData(@"\\.\C:\x.exe", "x.exe")]
    [InlineData(@"\\?\C:\x.exe", "x.exe")]
    [InlineData(@"\\?\C:/x.exe", "x.exe")]
    [InlineData(@"\\?\UNC\srv\sh", "")]
    [InlineData(@"\\?\UNC\srv\f.exe", "")]
    [InlineData(@"\\?\UNC\srv\sh\f.exe", "f.exe")]
    [InlineData(@"\\?\unc\srv\f.exe", "f.exe")] // only the exact "UNC" label is a UNC device root
    [InlineData(@"\\.\pipe\x", "x")]
    [InlineData(@"\\.\pipe\", "")]
    [InlineData(@"\\.\pipe", "")]
    [InlineData(@"\??\Code.exe", "")]
    [InlineData(@"\Code.exe", "Code.exe")]
    public void Windows_GetFileName_NeverReturnsAComponentInsideTheRoot(string normalized, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.GetFileName(normalized));
    }

    // The retained file-name fallback must not match a root component (HEAD: Path.GetFileName is empty there).
    [Theory]
    [InlineData(@"\\server\Code.exe")]
    [InlineData(@"\\.\Code.exe")]
    [InlineData(@"\\?\Code.exe")]
    [InlineData(@"\??\Code.exe")]
    [InlineData(@"\\?\UNC\srv\Code.exe")]
    public void EvaluateApplicationStart_Denies_RootComponentThatLooksLikeTheAllowedFileName(string candidate)
    {
        Assert.False(_sut.EvaluateApplicationStart(candidate, App(@"C:\Apps\Code.exe")).IsAllowed);
    }

    // Device paths: ".." cannot climb out of an allowed device-form folder (HEAD: deny).
    [Theory]
    [InlineData(@"\\.\C:\Study\Math", true)] // control
    [InlineData(@"\\.\C:\Study\..\..\Windows", false)]
    [InlineData(@"\\.\C:\Study\..\Other", false)]
    [InlineData(@"\\.\C:\Study\..\Study\Math", true)]
    [InlineData(@"//./C:/Study/../../Windows", false)]
    public void EvaluateFolderOpen_DevicePathFolder_DotDotCannotEscape(string candidate, bool expectedAllowed)
    {
        Assert.Equal(expectedAllowed, _sut.EvaluateFolderOpen(candidate, Folder(@"\\.\C:\Study", true)).IsAllowed);
    }

    [Fact]
    public void Unix_Normalize_DoesNotTrimDotsOrSpaces()
    {
        Assert.Equal("/home/study.", PathRules.Unix.Normalize("/home/study."));
        Assert.Equal("/home/study /x", PathRules.Unix.Normalize("/home/study /x"));
        Assert.Equal("/home/study./x", PathRules.Unix.Normalize("/home/study./x"));
        Assert.Equal("/home/a:b", PathRules.Unix.Normalize("/home/a:b"));
        Assert.Equal("a:b", PathRules.Unix.GetFileName("/home/a:b"));
        Assert.Equal("c:b", PathRules.Unix.GetFileName("c:b"));
    }
}
