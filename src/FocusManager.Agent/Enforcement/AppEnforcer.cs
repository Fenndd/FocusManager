using System.Diagnostics;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.Logging;

namespace FocusManager.Agent.Enforcement;

public sealed class AppEnforcer
{
    private readonly INotifier _notifier;
    private readonly RuleEvaluator _ruleEvaluator;
    private readonly ILogger<AppEnforcer> _logger;

    public AppEnforcer(RuleEvaluator ruleEvaluator, INotifier notifier, ILogger<AppEnforcer> logger)
    {
        _ruleEvaluator = ruleEvaluator;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task EnforceAsync(
        ProcessStartedEventArgs args,
        WhitelistConfig config,
        ISet<int>? exemptProcessIds = null,
        CancellationToken cancellationToken = default)
    {
        if (exemptProcessIds is not null && exemptProcessIds.Contains(args.ProcessId))
        {
            return;
        }

        var decision = _ruleEvaluator.EvaluateApplicationStart(args.ExecutablePath, config);
        if (decision.IsAllowed)
        {
            return;
        }

        if (!TryTerminateProcess(args.ProcessId))
        {
            return;
        }

        var blockedTarget = string.IsNullOrWhiteSpace(args.ExecutablePath)
            ? $"PID {args.ProcessId}"
            : args.ExecutablePath;

        _logger.LogInformation("Blocked application launch: {Target}. Reason: {Reason}", blockedTarget, decision.Reason);

        await _notifier.ShowBlockedAsync(
            "Blocked Application",
            $"{blockedTarget}. {decision.Reason}",
            cancellationToken);
    }

    private static bool TryTerminateProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);

            if (process.HasExited)
            {
                return false;
            }

            process.Kill(entireProcessTree: true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
