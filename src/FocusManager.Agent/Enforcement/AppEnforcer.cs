using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;

namespace FocusManager.Agent.Enforcement;

public sealed class AppEnforcer
{
    private readonly RuleEvaluator _ruleEvaluator;

    public AppEnforcer(RuleEvaluator ruleEvaluator)
    {
        _ruleEvaluator = ruleEvaluator;
    }

    public Task EnforceAsync(ProcessStartedEventArgs args, WhitelistConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Application enforcement is not implemented yet.");
    }
}
