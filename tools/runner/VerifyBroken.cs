namespace Sharplings.Runner;

public static class VerifyBroken
{
    public static Task<int> RunAsync(IReadOnlyList<Exercise> exercises, DotnetRunner runner)
        => Task.FromResult(0);
}
