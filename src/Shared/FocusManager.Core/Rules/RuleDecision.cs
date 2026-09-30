namespace FocusManager.Core.Rules;

public sealed record RuleDecision(bool IsAllowed, string Reason)
{
    public static RuleDecision Allow(string reason = "Allowed by whitelist") => new(true, reason);
    public static RuleDecision Deny(string reason = "Denied by whitelist") => new(false, reason);
}
