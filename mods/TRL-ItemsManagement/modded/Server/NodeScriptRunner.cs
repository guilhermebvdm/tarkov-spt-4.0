using System.Diagnostics;

namespace TRLItemsManagement;

/// <summary>
///     Runs one pipeline script (<c>tools/trl-items-management/scripts/*.js</c>) as a child process,
///     inheriting the current environment (so <c>TARKOV_MARKET_API_KEY</c>, if set on this machine,
///     reaches <c>fetch-tarkov-market.js</c>/<c>refresh-item.js</c>). Ported from <c>serve.js</c>'s
///     <c>runScript</c> helper. Resolves with the captured stderr tail on a non-zero exit, mirroring the
///     Node original's own truncated-error-message behavior (never dump megabytes of stderr into an API
///     response).
/// </summary>
internal static class NodeScriptRunner
{
    internal readonly record struct RunResult(int ExitCode, string StderrTail);

    internal static async Task<RunResult> RunAsync(string toolPath, string scriptName, params string[] args)
    {
        var scriptPath = Path.Combine(toolPath, "scripts", scriptName);
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            WorkingDirectory = toolPath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(scriptPath);
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stderrTask = process.StandardError.ReadToEndAsync();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stderr = await stderrTask;
        await stdoutTask;

        if (process.ExitCode == 0)
        {
            return new RunResult(0, string.Empty);
        }

        var lines = stderr.Trim().Split('\n');
        var tail = string.Join(" | ", lines.Length > 3 ? lines[^3..] : lines);
        return new RunResult(process.ExitCode, tail.Length > 400 ? tail[..400] : tail);
    }
}
