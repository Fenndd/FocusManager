using FocusManager.Core.Rules;
using Xunit;

namespace FocusManager.Core.Tests;

/// <summary>Direct tests of the pure-string path semantics; identical results are expected on every host OS.</summary>
public sealed class PathRulesTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(@"C:\Study", @"C:\Study")]
    [InlineData(@"C:\Study\", @"C:\Study")]
    [InlineData(@"C:\Study\\Math", @"C:\Study\Math")]
    [InlineData("C:/Study/Math", @"C:\Study\Math")]
    [InlineData("  \"C:\\Study\\Math\"  ", @"C:\Study\Math")]
    [InlineData(@"C:\Study\.\Math\..\Algebra", @"C:\Study\Algebra")]
    [InlineData(@"C:\..\..\Study", @"C:\Study")]
    [InlineData(@"C:\", @"C:\")]
    [InlineData("C:", "C:")]
    [InlineData(@"\\server\share\dir\", @"\\server\share\dir")]
    [InlineData(@"\\server\share\..\..\other", @"\\server\share\other")]
    [InlineData("notepad.exe", "notepad.exe")]
    [InlineData(@"..\notepad.exe", @"..\notepad.exe")]
    public void Windows_Normalize(string? input, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.Normalize(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("  ", "")]
    [InlineData("/home/user", "/home/user")]
    [InlineData("/home/user/", "/home/user")]
    [InlineData("/home//user///docs", "/home/user/docs")]
    [InlineData("/home/user/./docs/../notes", "/home/user/notes")]
    [InlineData("/../../etc", "/etc")]
    [InlineData("/", "/")]
    [InlineData("//", "/")]
    [InlineData("tool", "tool")]
    [InlineData("../tool", "../tool")]
    [InlineData(@"/home/user\docs", @"/home/user\docs")]
    [InlineData(@"C:\Study", @"C:\Study")]
    public void Unix_Normalize(string? input, string expected)
    {
        Assert.Equal(expected, PathRules.Unix.Normalize(input));
    }

    [Theory]
    [InlineData(@"C:\Windows\System32\notepad.exe", "notepad.exe")]
    [InlineData("notepad.exe", "notepad.exe")]
    [InlineData(@"C:\", "")]
    [InlineData("C:notepad.exe", "notepad.exe")]
    public void Windows_GetFileName(string normalized, string expected)
    {
        Assert.Equal(expected, PathRules.Windows.GetFileName(normalized));
    }

    [Theory]
    [InlineData("/usr/bin/tool", "tool")]
    [InlineData("tool", "tool")]
    [InlineData("/", "")]
    [InlineData(@"/dir/a\b", @"a\b")]
    public void Unix_GetFileName(string normalized, string expected)
    {
        Assert.Equal(expected, PathRules.Unix.GetFileName(normalized));
    }

    [Fact]
    public void AreEqual_UsesStyleCaseSensitivity()
    {
        Assert.True(PathRules.Windows.AreEqual(@"C:\Study", @"c:\STUDY"));
        Assert.False(PathRules.Unix.AreEqual("/home/Study", "/home/study"));
        Assert.True(PathRules.Unix.AreEqual("/home/Study", "/home/Study"));
    }

    [Theory]
    [InlineData(@"C:\Study\Math", @"C:\Study", true)]
    [InlineData(@"c:\study\math", @"C:\Study", true)]
    [InlineData(@"C:\Study", @"C:\Study", false)]
    [InlineData(@"C:\StudyOther", @"C:\Study", false)]
    [InlineData(@"C:\Study", @"C:\", true)]
    [InlineData(@"D:\Study", @"C:\", false)]
    [InlineData(@"C:\Study", "", false)]
    public void Windows_IsChild(string candidate, string parent, bool expected)
    {
        Assert.Equal(expected, PathRules.Windows.IsChild(candidate, parent));
    }

    [Theory]
    [InlineData("/home/user/docs", "/home/user", true)]
    [InlineData("/home/User/docs", "/home/user", false)]
    [InlineData("/home/user", "/home/user", false)]
    [InlineData("/home/username", "/home/user", false)]
    [InlineData("/home/user", "/", true)]
    [InlineData("/home", "", false)]
    [InlineData(@"/home/user\docs", "/home/user", false)]
    public void Unix_IsChild(string candidate, string parent, bool expected)
    {
        Assert.Equal(expected, PathRules.Unix.IsChild(candidate, parent));
    }
}
