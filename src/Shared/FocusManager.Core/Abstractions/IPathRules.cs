namespace FocusManager.Core.Abstractions;

/// <summary>
/// Explicit path semantics (separators, case sensitivity, normalization) used by rule evaluation.
/// Implementations are pure string logic: they must not consult the host OS, so the same input
/// gives the same result on every platform that runs the shared Core.
/// </summary>
public interface IPathRules
{
    /// <summary>
    /// Returns the canonical form of <paramref name="path"/>: trimmed, unquoted, with unified
    /// separators, collapsed duplicate separators, resolved <c>.</c>/<c>..</c> segments and no
    /// trailing separator (a root keeps its separator where the style has one). Never throws.
    /// Returns an empty string for null, empty or whitespace input.
    /// </summary>
    string Normalize(string? path);

    /// <summary>Returns the last path component of an already normalized path.</summary>
    string GetFileName(string normalizedPath);

    /// <summary>Compares two already normalized paths using the style's case sensitivity.</summary>
    bool AreEqual(string normalizedPath, string otherNormalizedPath);

    /// <summary>
    /// Returns true when <paramref name="normalizedCandidate"/> is strictly inside
    /// <paramref name="normalizedParent"/>. Both arguments must already be normalized.
    /// </summary>
    bool IsChild(string normalizedCandidate, string normalizedParent);
}
