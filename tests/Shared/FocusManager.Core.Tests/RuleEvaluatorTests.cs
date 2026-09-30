using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

public sealed class RuleEvaluatorTests
{
    private readonly RuleEvaluator _sut = new();

    [Fact]
    public void EvaluateApplicationStart_Denies_WhenWhitelistIsEmpty()
    {
        var decision = _sut.EvaluateApplicationStart(@"C:\Apps\Code.exe", new WhitelistConfig());

        Assert.False(decision.IsAllowed);
        Assert.Contains("empty", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateApplicationStart_Allows_ExactExecutablePathMatch_IgnoringCase()
    {
        var config = new WhitelistConfig
        {
            AllowedApps =
            [
                new AllowedApp("Code", @"C:\Program Files\Microsoft VS Code\Code.exe")
            ]
        };

        var decision = _sut.EvaluateApplicationStart(@"c:\program files\microsoft vs code\CODE.exe", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable()
    {
        var config = new WhitelistConfig
        {
            AllowedApps =
            [
                new AllowedApp("Notepad", @"C:\Windows\System32\notepad.exe")
            ]
        };

        var decision = _sut.EvaluateApplicationStart("notepad.exe", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateApplicationStart_Denies_WhenApplicationIsNotWhitelisted()
    {
        var config = new WhitelistConfig
        {
            AllowedApps =
            [
                new AllowedApp("Code", @"C:\Program Files\Microsoft VS Code\Code.exe")
            ]
        };

        var decision = _sut.EvaluateApplicationStart(@"C:\Apps\Discord.exe", config);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_Denies_WhenWhitelistIsEmpty()
    {
        var decision = _sut.EvaluateFolderOpen(@"C:\Study", new WhitelistConfig());

        Assert.False(decision.IsAllowed);
        Assert.Contains("empty", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateFolderOpen_Allows_SameFolderPath_IgnoringTrailingSeparator()
    {
        var config = new WhitelistConfig
        {
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\Study")
            ]
        };

        var decision = _sut.EvaluateFolderOpen(@"C:\Study\", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_Denies_ChildFolder_WhenSubfoldersNotAllowed()
    {
        var config = new WhitelistConfig
        {
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\Study")
            ]
        };

        var decision = _sut.EvaluateFolderOpen(@"C:\Study\Math\Algebra", config);

        Assert.False(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_Allows_ChildFolder_WhenSubfoldersAllowed()
    {
        var config = new WhitelistConfig
        {
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\Study", AllowSubfolders: true)
            ]
        };

        var decision = _sut.EvaluateFolderOpen(@"C:\Study\Math\Algebra", config);

        Assert.True(decision.IsAllowed);
    }

    [Fact]
    public void EvaluateFolderOpen_Denies_SiblingFolder()
    {
        var config = new WhitelistConfig
        {
            AllowedFolders =
            [
                new AllowedFolder("Study", @"C:\Study")
            ]
        };

        var decision = _sut.EvaluateFolderOpen(@"C:\StudyOther", config);

        Assert.False(decision.IsAllowed);
    }

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
