using FocusManager.Core.Abstractions;
using FocusManager.Core.Rules;

namespace FocusManager.Platform.Windows;

/// <summary>
/// Windows <see cref="IPathRules"/> for the agent: <see cref="PathRules.Windows"/> plus the host context that
/// the pre-T001 evaluator obtained implicitly from <c>Path.GetFullPath</c> (T054). A path that is not fully
/// qualified is resolved the same way before it is normalized, so values already persisted by the UI keep their
/// meaning: <c>D:</c> is the current directory of drive D: (its root unless the process changed it), a leading
/// <c>\Study</c> belongs to the current drive, and a plain relative name belongs to the current directory.
/// A fully qualified path is normalized purely lexically and consults the host only when it contains <c>~</c>
/// (8.3 short names, which only the file system can expand).
/// </summary>
/// <remarks>
/// Shared Core must not read the current directory or drive; this class is the explicit place where that
/// Windows state enters rule evaluation. It is registered in <c>Program.cs</c> of the agent.
/// </remarks>
public sealed class WindowsHostPathRules : IPathRules
{
    private readonly IPathRules _inner;
    private readonly Func<string, string> _resolveFullPath;

    /// <summary>Uses <see cref="PathRules.Windows"/> and the real <see cref="Path.GetFullPath(string)"/>.</summary>
    public WindowsHostPathRules()
        : this(PathRules.Windows, Path.GetFullPath)
    {
    }

    /// <param name="inner">Lexical Windows path rules.</param>
    /// <param name="resolveFullPath">Resolves a not fully qualified path against the host's current drive and
    /// per-drive current directories; replaceable so tests can supply a synthetic host.</param>
    public WindowsHostPathRules(IPathRules inner, Func<string, string> resolveFullPath)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _resolveFullPath = resolveFullPath ?? throw new ArgumentNullException(nameof(resolveFullPath));
    }

    public string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');
        // A "~" may be an 8.3 short name ("C:\PROGRA~1"), which only the file system can expand; Path.GetFullPath did so
        // before T001, so a fully qualified path containing "~" still goes through the host resolver.
        if (!Path.IsPathFullyQualified(cleaned) || cleaned.Contains('~'))
        {
            try
            {
                cleaned = _resolveFullPath(cleaned);
            }
            catch
            {
                // Same best-effort behavior as before: keep the raw value when resolution fails.
            }
        }

        return _inner.Normalize(cleaned);
    }

    public string GetFileName(string normalizedPath) => _inner.GetFileName(normalizedPath);

    public bool AreEqual(string normalizedPath, string otherNormalizedPath) => _inner.AreEqual(normalizedPath, otherNormalizedPath);

    public bool IsChild(string normalizedCandidate, string normalizedParent) => _inner.IsChild(normalizedCandidate, normalizedParent);
}
