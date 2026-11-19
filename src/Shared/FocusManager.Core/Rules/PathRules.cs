using FocusManager.Core.Abstractions;

namespace FocusManager.Core.Rules;

/// <summary>
/// Pure-string <see cref="IPathRules"/> implementations for the Windows and Unix path styles.
/// Does not call the BCL path helpers, the current directory or any other host-dependent state.
/// Relative paths are never resolved against a working directory; they are only normalized.
/// Windows style also reproduces the lexical part of Win32 path normalization that the pre-T001 evaluator
/// obtained from <c>Path.GetFullPath</c> (trailing dots and spaces, device-path roots); a path with the literal
/// verbatim prefix <c>\\?\</c> is left as written, like Win32 does. Host-dependent resolution (drive-relative and
/// root-relative paths, 8.3 short names) is supplied by a platform adapter instead (T054).
/// </summary>
public sealed class PathRules : IPathRules
{
    private readonly bool _isWindows;
    private readonly char _separator;
    private readonly StringComparison _comparison;

    /// <summary>Windows style: <c>\</c> and <c>/</c> are separators, drive and UNC roots, case insensitive.</summary>
    public static PathRules Windows { get; } = new(isWindows: true);

    /// <summary>Unix style: only <c>/</c> is a separator (<c>\</c> is an ordinary character), case sensitive.</summary>
    public static PathRules Unix { get; } = new(isWindows: false);

    private PathRules(bool isWindows)
    {
        _isWindows = isWindows;
        _separator = isWindows ? '\\' : '/';
        _comparison = isWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    }

    public string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var cleaned = path.Trim().Trim('"');
        if (_isWindows)
        {
            if (IsVerbatim(cleaned))
            {
                // Win32 does not normalize "\\?\" paths: dot segments, doubled separators, forward slashes and
                // trailing dots are all literal, so only trailing separators are trimmed (as the pre-T001 evaluator did).
                return cleaned.TrimEnd('\\', '/');
            }

            cleaned = cleaned.Replace('/', '\\');
        }

        var (root, protectedSegments) = SplitRoot(cleaned, out var rest, out var emptyShare);
        var isRooted = root.Length > 0 && (root[^1] == _separator);

        var segments = new List<string>();
        foreach (var segment in rest.Split(_separator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (segments.Count < protectedSegments)
            {
                // The server and share of a UNC root are literal names (Win32/.NET): "." and ".." there are not dot
                // segments, are never skipped or trimmed here, and a later ".." cannot remove them.
                segments.Add(segment);
                continue;
            }

            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count > protectedSegments && segments[^1] != "..")
                {
                    segments.RemoveAt(segments.Count - 1);
                }
                else if (!isRooted)
                {
                    segments.Add(segment);
                }

                // Rooted paths cannot go above their root: the extra ".." is dropped.
                continue;
            }

            var name = _isWindows ? TrimInnerSegment(segment) : segment;
            if (name.Length > 0)
            {
                segments.Add(name);
            }
        }

        // Win32 also trims the final component when it is the UNC share or server ("\\srv\share." is "\\srv\share"),
        // but not the server of "\\srv\\" (empty share): there the final component is the empty share.
        if (_isWindows && segments.Count > (emptyShare ? protectedSegments : 0) && !EndsWithSeparator(cleaned))
        {
            TrimFinalSegment(segments);
        }

        return root + string.Join(_separator, segments);
    }

    public string GetFileName(string normalizedPath)
    {
        if (!_isWindows)
        {
            return normalizedPath[(normalizedPath.LastIndexOf(_separator) + 1)..];
        }

        // Same rule as Path.GetFileName on Windows: the name never starts inside the root, so a UNC server or share,
        // a device or verbatim volume ("\\?\C:", "\\.\pipe\") and a drive prefix are not file names. A colon is
        // therefore a boundary only as the drive-prefix colon ("C:name.exe"); a colon elsewhere in the final
        // component (for example an alternate-data-stream style "evil.exe:Code.exe") is part of the name.
        var separator = normalizedPath.AsSpan().LastIndexOfAny('\\', '/');
        var root = WindowsRootLength(normalizedPath);
        return normalizedPath[(separator < root ? root : separator + 1)..];
    }

    public bool AreEqual(string normalizedPath, string otherNormalizedPath) =>
        string.Equals(normalizedPath, otherNormalizedPath, _comparison);

    public bool IsChild(string normalizedCandidate, string normalizedParent)
    {
        if (normalizedParent.Length == 0)
        {
            return false;
        }

        var prefix = normalizedParent[^1] == _separator
            ? normalizedParent
            : normalizedParent + _separator;

        return normalizedCandidate.StartsWith(prefix, _comparison);
    }

    /// <summary>
    /// Splits off the root. <paramref name="protectedSegments"/> is the number of leading segments
    /// that belong to the root (the server and share of a UNC path) and must not be removed by "..".
    /// </summary>
    private (string Root, int ProtectedSegments) SplitRoot(string path, out string rest, out bool emptyShare)
    {
        emptyShare = false;
        if (!_isWindows)
        {
            if (path.StartsWith('/'))
            {
                rest = path;
                return ("/", 0);
            }

            rest = path;
            return (string.Empty, 0);
        }

        if (HasDrivePrefix(path))
        {
            // Drive root "C:\" (separator kept) or drive-relative "C:" (no separator).
            var hasSeparator = path.Length > 2 && path[2] == '\\';
            rest = path[(hasSeparator ? 3 : 2)..];
            return (hasSeparator ? path[..3] : path[..2], 0);
        }

        if (path.Length >= 4 && IsDevicePrefix(path))
        {
            // Device root "\\.\" (also "\\?\" written with forward slashes): only the four-character prefix is the
            // root. What follows, including a volume such as "C:" or "pipe", is an ordinary segment that ".." can remove.
            rest = path[4..];
            return (path[..4], 0);
        }

        if (path == @"\\.")
        {
            rest = string.Empty;
            return (@"\\.\", 0);
        }

        if (path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            // UNC: "\\server\share" - the server and share segments stay with the root. A doubled separator right
            // after the server ("\\server\\x") makes the share empty, so only the server is a root segment.
            rest = path[2..];
            var serverEnd = rest.IndexOf('\\');
            emptyShare = serverEnd >= 0 && serverEnd + 1 < rest.Length && rest[serverEnd + 1] == '\\';
            if (serverEnd == 0)
            {
                // A third leading separator is the empty UNC server slot; keep it in the root before removing
                // empty segments. With exactly three, the next name is the literal share. With four or more,
                // the share is also empty, so subsequent names are ordinary segments (Win32/.NET behavior).
                rest = rest[1..];
                return (@"\\\", emptyShare ? 0 : 1);
            }

            return (@"\\", emptyShare ? 1 : 2);
        }

        if (path.StartsWith('\\'))
        {
            rest = path;
            return (@"\", 0);
        }

        rest = path;
        return (string.Empty, 0);
    }

    /// <summary>The literal verbatim prefix <c>\\?\</c> (or the NT form <c>\??\</c>), which Win32 does not normalize.</summary>
    private static bool IsVerbatim(string path) =>
        path.StartsWith(@"\\?\", StringComparison.Ordinal) || path.StartsWith(@"\??\", StringComparison.Ordinal);

    /// <summary>Device prefix <c>\\.\</c>, <c>\\?\</c> or <c>\??\</c> written with backslashes (length is at least four).</summary>
    private static bool IsDevicePrefix(string path) =>
        path[0] == '\\' && path[3] == '\\'
        && ((path[1] == '\\' && path[2] is '.' or '?') || (path[1] == '?' && path[2] == '?'));

    private static bool IsSeparator(char value) => value is '\\' or '/';

    /// <summary>
    /// Length of the root as <c>Path.GetPathRoot</c> measures it on Windows, computed lexically: <c>C:</c> or
    /// <c>C:\</c>, <c>\</c>, <c>\\server\share</c>, <c>\\?\UNC\server\share</c>, and a device prefix plus its first
    /// segment (<c>\\.\pipe\</c>, <c>\\?\C:\</c>).
    /// </summary>
    private static int WindowsRootLength(string path)
    {
        var length = path.Length;
        var device = length >= 4 && IsDevicePrefix(path);
        var deviceUnc = device && length >= 8 && IsSeparator(path[7]) && path.AsSpan(4, 3).SequenceEqual("UNC");

        if ((!device || deviceUnc) && length > 0 && IsSeparator(path[0]))
        {
            if (!deviceUnc && !(length > 1 && IsSeparator(path[1])))
            {
                return 1; // "\name": the root of the current drive
            }

            // UNC ("\\server\share" or "\\?\UNC\server\share"): the root spans the server and the share.
            var i = deviceUnc ? 8 : 2;
            var separators = 2;
            while (i < length && (!IsSeparator(path[i]) || --separators > 0))
            {
                i++;
            }

            return i;
        }

        if (device)
        {
            var i = 4;
            while (i < length && !IsSeparator(path[i]))
            {
                i++;
            }

            return i < length && i > 4 ? i + 1 : i;
        }

        if (HasDrivePrefix(path))
        {
            return length > 2 && IsSeparator(path[2]) ? 3 : 2;
        }

        return 0;
    }

    private static bool HasDrivePrefix(string path) =>
        path.Length >= 2 && IsAsciiLetter(path[0]) && path[1] == ':';

    private static bool EndsWithSeparator(string path) =>
        path.Length > 0 && (path[^1] == '\\' || path[^1] == '/');

    /// <summary>
    /// Win32 drops one trailing dot from a component that is followed by a separator ("Study.\Math" is
    /// "Study\Math"); a run of two or more dots, trailing spaces and other names are kept.
    /// </summary>
    private static string TrimInnerSegment(string segment) =>
        segment.Length > 0 && segment[^1] == '.' && (segment.Length < 2 || segment[^2] != '.')
            ? segment[..^1]
            : segment;

    /// <summary>
    /// Win32 drops all trailing dots and spaces from the final component of a path that does not end in a
    /// separator; a component that becomes empty disappears ("C:\p\..." is "C:\p").
    /// </summary>
    private static void TrimFinalSegment(List<string> segments)
    {
        var last = segments[^1].TrimEnd(' ', '.');
        if (last.Length == 0)
        {
            segments.RemoveAt(segments.Count - 1);
        }
        else
        {
            segments[^1] = last;
        }
    }

    private static bool IsAsciiLetter(char value) =>
        value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
