using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AIContextManager;

public sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
{
    public const int NotFound = -1;
    public const int TimedOut = -2;
    public bool Success => ExitCode == 0;
}

/// <summary>Thin wrapper around System.Diagnostics.Process. No shell is involved; arguments are passed as a list.</summary>
public static partial class ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory,
        TimeSpan timeout,
        CancellationToken ct,
        Func<Stream, CancellationToken, Task>? stdoutConsumer = null)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = stdoutConsumer is null ? Encoding.UTF8 : null,
            StandardErrorEncoding = Encoding.UTF8,
        };
        if (workingDirectory is not null) psi.WorkingDirectory = workingDirectory;
        foreach (var a in arguments) psi.ArgumentList.Add(a);

        // Never block on a credential prompt; never colour output we parse.
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["GCM_INTERACTIVE"] = "never";
        psi.Environment["NO_COLOR"] = "1";
        psi.Environment["FORCE_COLOR"] = "0";

        using var process = new Process { StartInfo = psi };
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            return new ProcessResult(ProcessResult.NotFound, "", $"Could not start '{fileName}': {ex.Message}");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var stderrTask = process.StandardError.ReadToEndAsync(cts.Token);
        try
        {
            var stdout = "";
            if (stdoutConsumer is null)
            {
                stdout = await process.StandardOutput.ReadToEndAsync(cts.Token);
            }
            else
            {
                await stdoutConsumer(process.StandardOutput.BaseStream, cts.Token);
                await process.StandardOutput.BaseStream.CopyToAsync(Stream.Null, cts.Token);
            }

            await process.WaitForExitAsync(cts.Token);
            return new ProcessResult(process.ExitCode, stdout, await stderrTask);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            Kill(process);
            return new ProcessResult(ProcessResult.TimedOut, "", $"'{fileName}' timed out after {timeout.TotalMinutes:0.#} min.");
        }
        catch
        {
            Kill(process);
            throw;
        }
    }

    /// <summary>Removes credentials embedded in URLs (https://user:token@host) and trims long text before logging.</summary>
    public static string SafeForLog(string? text, int max = 400)
    {
        var clean = UrlCredentials().Replace(text ?? "", "://***@").Trim();
        return clean.Length <= max ? clean : clean[..max] + "...";
    }

    private static void Kill(Process p)
    {
        try { if (!p.HasExited) p.Kill(entireProcessTree: true); }
        catch { /* already gone */ }
    }

    [GeneratedRegex(@"://[^/\s@]+@")]
    private static partial Regex UrlCredentials();
}
