using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIContextManager;

public sealed record RepomixResult(bool Success, long? TotalTokens, string? Error);

/// <summary>Runs the Repomix CLI once per snapshot. Repomix itself respects .gitignore, .repomixignore and its own security check.</summary>
public sealed partial class RepomixService(IOptions<ContextManagerOptions> options, ILogger<RepomixService> log)
{
    /// <summary>Always ignored, on top of .gitignore and .repomixignore.</summary>
    public static readonly string[] DefaultIgnorePatterns =
    [
        "**/.git/**", "**/.vs/**", "**/bin/**", "**/obj/**", "**/node_modules/**", "**/dist/**", "**/build/**",
        "**/coverage/**", "**/TestResults/**", "**/*.tmp", "**/*.temp", "**/*.log", "**/*.min.js", "**/*.map",
        "**/package-lock.json", "**/yarn.lock", "**/pnpm-lock.yaml",
        "**/DEV-context.md", "**/PROD-context.md", "**/*-context.md", "**/repomix-output.*", "**/00-INDEX.md",
    ];

    private readonly ContextManagerOptions _o = options.Value;
    private (string File, string[] Prefix)? _command;

    public IReadOnlyList<string> IgnorePatterns => [.. DefaultIgnorePatterns, .. _o.ExtraIgnorePatterns];

    public async Task<string?> GetVersionAsync(CancellationToken ct)
    {
        var (file, prefix) = Command;
        var r = await ProcessRunner.RunAsync(file, [.. prefix, "--version"], null, TimeSpan.FromMinutes(1), ct);
        if (r.Success) return r.StdOut.Trim();
        log.LogDebug("repomix --version failed: {Error}", ProcessRunner.SafeForLog(r.StdErr));
        return null;
    }

    public async Task<RepomixResult> RunAsync(string snapshotDir, string outputFile, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        if (File.Exists(outputFile)) File.Delete(outputFile);

        var (file, prefix) = Command;
        string[] args =
        [
            .. prefix, ".",
            "--style", "markdown",
            "--no-file-summary", // drop Repomix's generic boilerplate; our header carries the metadata
            "--output", outputFile,
            "--ignore", string.Join(',', IgnorePatterns),
        ];

        var r = await ProcessRunner.RunAsync(file, args, snapshotDir, TimeSpan.FromMinutes(_o.RepomixTimeoutMinutes), ct);
        if (!r.Success || !File.Exists(outputFile))
        {
            var detail = string.IsNullOrWhiteSpace(r.StdErr) ? r.StdOut : r.StdErr;
            return new RepomixResult(false, null, $"repomix exit code {r.ExitCode}: {ProcessRunner.SafeForLog(detail)}");
        }

        var match = TotalTokens().Match(r.StdOut);
        long? tokens = match.Success && long.TryParse(match.Groups[1].Value.Replace(",", ""), out var t) ? t : null;
        return new RepomixResult(true, tokens, null);
    }

    private (string File, string[] Prefix) Command => _command ??= ResolveCommand(_o.RepomixCommand);

    /// <summary>
    /// "repomix" installed with npm on Windows is a .cmd shim. Process.Start cannot run .cmd files directly, so we
    /// prefer "node &lt;repomix.cjs&gt;" (no shell, no quoting issues) and fall back to "cmd.exe /d /c &lt;shim&gt;".
    /// </summary>
    internal static (string File, string[] Prefix) ResolveCommand(string command)
    {
        var resolved = FindOnPath(command);
        if (resolved is null) return (command, []);

        var ext = Path.GetExtension(resolved);
        if (!ext.Equals(".cmd", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".bat", StringComparison.OrdinalIgnoreCase))
            return (resolved, []);

        // Global install: %APPDATA%\npm\repomix.cmd + node_modules\repomix. Local install: node_modules\.bin\repomix.cmd.
        var shimDir = Path.GetDirectoryName(resolved)!;
        var script = new[]
            {
                Path.Combine(shimDir, "node_modules", "repomix", "bin", "repomix.cjs"),
                Path.Combine(shimDir, "..", "repomix", "bin", "repomix.cjs"),
            }
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);
        var node = FindOnPath("node");
        if (script is not null && node is not null) return (node, [script]);

        return ("cmd.exe", ["/d", "/c", resolved]);
    }

    private static string? FindOnPath(string command)
    {
        string[] extensions = OperatingSystem.IsWindows()
            ? ["", .. (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)]
            : [""];

        IEnumerable<string> candidates = Path.IsPathRooted(command) || command.Contains(Path.DirectorySeparatorChar)
            ? extensions.Select(e => command + e)
            : (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(dir => extensions.Select(e => Path.Combine(dir.Trim('"'), command + e)));

        foreach (var c in candidates)
        {
            try
            {
                // On Windows a bare "repomix" (no extension) is the POSIX shell script; skip it.
                if (File.Exists(c) && (!OperatingSystem.IsWindows() || Path.HasExtension(c))) return c;
            }
            catch (ArgumentException) { }
        }

        return null;
    }

    [GeneratedRegex(@"Total Tokens:\s*([\d,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TotalTokens();
}
