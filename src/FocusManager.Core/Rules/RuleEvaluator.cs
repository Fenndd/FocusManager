using FocusManager.Core.Models;

namespace FocusManager.Core.Rules;

public sealed class RuleEvaluator
{
    public RuleDecision EvaluateApplicationStart(string executablePath, WhitelistConfig config)
    {
        throw new NotImplementedException("App whitelist evaluation is not implemented yet.");
    }

    public RuleDecision EvaluateFolderOpen(string folderPath, WhitelistConfig config)
    {
        throw new NotImplementedException("Folder whitelist evaluation is not implemented yet.");
    }

    public RuleDecision EvaluateSiteOpen(string urlOrHost, WhitelistConfig config)
    {
        throw new NotImplementedException("Site whitelist evaluation is not implemented yet.");
    }
}
