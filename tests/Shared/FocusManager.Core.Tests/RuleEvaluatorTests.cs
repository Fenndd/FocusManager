using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

public sealed class RuleEvaluatorTests
{
    private readonly RuleEvaluator _sut = new(PathRules.Windows);

    public static TheoryData<string> Styles => PathStyleFixture.Names;

    private static WhitelistConfig Apps(params AllowedApp[] apps) => new() { AllowedApps = [.. apps] };

    private static WhitelistConfig Folders(params AllowedFolder[] folders) => new() { AllowedFolders = [.. folders] };

    [Fact]
    public void Constructor_Throws_WhenPathRulesAreMissing()
    {
        Assert.Throws<ArgumentNullException>(() => new RuleEvaluator(null!));
    }

    // ---- Applications: run for both Windows and Unix path styles --------------------------------

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateApplicationStart_Denies_WhenWhitelistIsEmpty(string style)
    {
        var fx = PathStyleFixture.For(style);

        var decision = fx.CreateEvaluator().EvaluateApplicationStart(fx.AbsolutePath("Apps", "Code.exe"), new WhitelistConfig());

        Assert.False(decision.IsAllowed);
        Assert.Contains("empty", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateApplicationStart_Allows_ExactExecutablePathMatch(string style)
    {
        var fx = PathStyleFixture.For(style);
        var path = fx.AbsolutePath("Program Files", "Microsoft VS Code", "Code.exe");

        var decision = fx.CreateEvaluator().EvaluateApplicationStart(path, Apps(new AllowedApp("Code", path)));

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateApplicationStart_Allows_ExactExecutablePathMatch_IgnoringCase_OnWindows()
    {
        var config = Apps(new AllowedApp("Code", @"C:\Program Files\Microsoft VS Code\Code.exe"));

        var decision = _sut.EvaluateApplicationStart(@"c:\program files\microsoft vs code\CODE.exe", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateApplicationStart_Denies_CaseDifference_OnUnix()
    {
        var fx = PathStyleFixture.For(PathStyleFixture.UnixName);
        var config = Apps(new AllowedApp("Code", "/opt/code/Code"));

        // Neither the full path nor the file name matches on a case-sensitive style.
        var decision = fx.CreateEvaluator().EvaluateApplicationStart("/OPT/code/CODE", config);

        Assert.False(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Apps(new AllowedApp("Notepad", fx.AbsolutePath("Windows", "System32", "notepad.exe")));

        var decision = fx.CreateEvaluator().EvaluateApplicationStart("notepad.exe", config);

        Assert.True(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateApplicationStart_Denies_WhenApplicationIsNotWhitelisted(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Apps(new AllowedApp("Code", fx.AbsolutePath("Program Files", "Microsoft VS Code", "Code.exe")));

        var decision = fx.CreateEvaluator().EvaluateApplicationStart(fx.AbsolutePath("Apps", "Discord.exe"), config);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateApplicationStart_NormalizesQuotesForwardSlashesAndDotSegments_OnWindows()
    {
        var config = Apps(new AllowedApp("Code", @"C:\Apps\Code.exe"));

        var decision = _sut.EvaluateApplicationStart("\"C:/Apps/./Tools/../Code.exe\"", config);

        Assert.True(decision.IsAllowed);
    }

    // ---- Folders: run for both Windows and Unix path styles -------------------------------------

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Denies_WhenWhitelistIsEmpty(string style)
    {
        var fx = PathStyleFixture.For(style);

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study"), new WhitelistConfig());

        Assert.False(decision.IsAllowed);
        Assert.Contains("empty", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Allows_SameFolderPath_IgnoringTrailingSeparator(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Study", fx.AbsolutePath("Study")));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study") + fx.Separator, config);

        Assert.True(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Denies_ChildFolder_WhenSubfoldersNotAllowed(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Study", fx.AbsolutePath("Study")));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study", "Math", "Algebra"), config);

        Assert.False(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Allows_ChildFolder_WhenSubfoldersAllowed(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Study", fx.AbsolutePath("Study"), AllowSubfolders: true));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study", "Math", "Algebra"), config);

        Assert.True(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Denies_SiblingFolder(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Study", fx.AbsolutePath("Study"), AllowSubfolders: true));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("StudyOther"), config);

        Assert.False(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_AllowsOnlyTheRootItself_WhenRootEntryHasNoSubfolders(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Root", fx.Root, AllowSubfolders: false));
        var sut = fx.CreateEvaluator();

        Assert.True(sut.EvaluateFolderOpen(fx.Root, config).IsAllowed);
        Assert.False(sut.EvaluateFolderOpen(fx.AbsolutePath("Study"), config).IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Allows_ChildOfRoot_WhenSubfoldersAllowed(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Root", fx.Root, AllowSubfolders: true));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study"), config);

        Assert.True(decision.IsAllowed);
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EvaluateFolderOpen_Denies_DotDotEscapeFromAllowedFolder(string style)
    {
        var fx = PathStyleFixture.For(style);
        var config = Folders(new AllowedFolder("Study", fx.AbsolutePath("Study"), AllowSubfolders: true));

        var decision = fx.CreateEvaluator().EvaluateFolderOpen(fx.AbsolutePath("Study", "..", "Games"), config);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_Allows_IgnoringCaseAndSlashStyle_OnWindows()
    {
        var config = Folders(new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true));

        Assert.True(_sut.EvaluateFolderOpen("c:/STUDY/", config).IsAllowed);
        Assert.True(_sut.EvaluateFolderOpen(@"C:\study\Math", config).IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_IsCaseSensitive_OnUnix()
    {
        var fx = PathStyleFixture.For(PathStyleFixture.UnixName);
        var config = Folders(new AllowedFolder("Study", "/home/user/Study", AllowSubfolders: true));
        var sut = fx.CreateEvaluator();

        Assert.False(sut.EvaluateFolderOpen("/home/user/study", config).IsAllowed);
        Assert.False(sut.EvaluateFolderOpen("/home/user/study/Math", config).IsAllowed);
        Assert.True(sut.EvaluateFolderOpen("/home/user/Study/Math", config).IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_TreatsBackslashAsOrdinaryCharacter_OnUnix()
    {
        var fx = PathStyleFixture.For(PathStyleFixture.UnixName);
        var config = Folders(new AllowedFolder("Study", "/home/user/Study", AllowSubfolders: true));

        // On Unix a backslash is part of a file name, so this is a sibling entry, not a child.
        Assert.False(fx.CreateEvaluator().EvaluateFolderOpen(@"/home/user/Study\Math", config).IsAllowed);
    }

    // ---- Sites: independent of the path style ----------------------------------------------------

    [Fact]
    public void EvaluateSiteOpen_Denies_WhenWhitelistIsEmpty()
    {
        var decision = _sut.EvaluateSiteOpen("wikipedia.org", new WhitelistConfig());

        Assert.False(decision.IsAllowed);
        Assert.Contains("empty", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateSiteOpen_Allows_ExactHost_FromUrlInput()
    {
        var config = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Wikipedia", "wikipedia.org")
            ]
        };

        var decision = _sut.EvaluateSiteOpen("https://wikipedia.org/wiki/Focus", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateSiteOpen_Allows_WildcardPattern_ForSubdomainAndRoot()
    {
        var config = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Docs", "*.example.com")
            ]
        };

        var subdomainDecision = _sut.EvaluateSiteOpen("docs.example.com", config);
        var rootDecision = _sut.EvaluateSiteOpen("example.com", config);

        Assert.True(subdomainDecision.IsAllowed);
        Assert.True(rootDecision.IsAllowed);
    }

    [Fact]
    public void EvaluateSiteOpen_Denies_InvalidHost()
    {
        var config = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Wikipedia", "wikipedia.org")
            ]
        };

        var decision = _sut.EvaluateSiteOpen("invalid host value", config);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateSiteOpen_Denies_NotWhitelistedHost()
    {
        var config = new WhitelistConfig
        {
            AllowedSites =
            [
                new AllowedSite("Wikipedia", "wikipedia.org")
            ]
        };

        var decision = _sut.EvaluateSiteOpen("github.com", config);

        Assert.False(decision.IsAllowed);
    }
}
