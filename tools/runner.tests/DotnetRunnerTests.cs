using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

// These tests execute the real dotnet CLI (no mocks). First run of each
// fixture compiles it (~5 s); afterwards the file-based-app cache makes
// re-runs fast.
public class DotnetRunnerTests
{
    private static async Task<RunResult> Run(string fixture, double timeoutSeconds = 120)
    {
        string root = Fixtures.Path("running");
        var exercise = Exercise.Parse(root,
            Fixtures.Path($"running/exercises/00_run/{fixture}"));
        return await new DotnetRunner(TimeSpan.FromSeconds(timeoutSeconds))
            .RunAsync(exercise);
    }

    [Fact]
    public async Task Matching_output_is_pass()
    {
        var result = await Run("001_ok.cs");
        Assert.Equal(Verdict.Pass, result.Verdict);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Nonmatching_output_is_mismatch()
    {
        var result = await Run("002_wrong_output.cs");
        Assert.Equal(Verdict.OutputMismatch, result.Verdict);
    }

    [Fact]
    public async Task Build_failure_is_compile_error()
    {
        var result = await Run("003_compile_error.cs");
        Assert.Equal(Verdict.CompileError, result.Verdict);
        // The CSxxxx diagnostic lands in stdout (measured behavior).
        Assert.Contains("error CS", result.Stdout);
    }

    [Fact]
    public async Task Unhandled_exception_is_runtime_error()
    {
        var result = await Run("004_crash.cs");
        Assert.Equal(Verdict.RuntimeError, result.Verdict);
        Assert.Contains("InvalidOperationException", result.Stderr);
    }

    [Fact]
    public async Task Infinite_loop_times_out()
    {
        // Generous enough to cover first-time compilation of the fixture,
        // short enough to keep the suite tolerable.
        var result = await Run("005_loop.cs", timeoutSeconds: 30);
        Assert.Equal(Verdict.Timeout, result.Verdict);
    }
}
