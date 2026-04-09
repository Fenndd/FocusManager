using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Enforcement;

public sealed class FolderEnforcer
{
    private readonly RuleEvaluator _ruleEvaluator;
    private readonly ExplorerInterop _explorerInterop;

    public FolderEnforcer(RuleEvaluator ruleEvaluator, ExplorerInterop explorerInterop)
    {
        _ruleEvaluator = ruleEvaluator;
        _explorerInterop = explorerInterop;
    }

    public Task EnforceAsync(FolderOpenedEventArgs args, WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Folder enforcement is not implemented yet.");
    }
}
