using System.Diagnostics;

namespace Sharplings.Runner;

public enum Verdict { Pass, CompileError, RuntimeError, OutputMismatch, Timeout }

public sealed record RunResult(Verdict Verdict, string Stdout, string Stderr, int ExitCode);

/// <summary>Runs one exercise as a .NET 10 file-based app and classifies the outcome.</summary>
public sealed class DotnetRunner(TimeSpan timeout)
{
    public async Task<RunResult> RunAsync(Exercise exercise, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "run", exercise.AbsolutePath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(exercise.AbsolutePath)!,
        };
        // Keep diagnostics locale-stable so verdict classification works
        // regardless of the user's OS language.
        psi.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
        psi.Environment["DOTNET_NOLOGO"] = "1";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            return new RunResult(Verdict.Timeout, await stdoutTask, await stderrTask, -1);
        }

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        Verdict verdict = process.ExitCode switch
        {
            0 => OutputMatches(stdout, exercise.ExpectedOutput)
                ? Verdict.Pass
                : Verdict.OutputMismatch,
            _ => stderr.Contains("The build failed")
                ? Verdict.CompileError
                : Verdict.RuntimeError,
        };
        return new RunResult(verdict, stdout, stderr, process.ExitCode);
    }

    public static bool OutputMatches(string actual, string expected)
    {
        static string Normalize(string s) => string.Join('\n',
            s.ReplaceLineEndings("\n").Split('\n').Select(static l => l.TrimEnd()))
            .TrimEnd('\n');
        return Normalize(actual) == Normalize(expected);
    }
}
