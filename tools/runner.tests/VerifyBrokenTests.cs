using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class VerifyBrokenTests
{
    private static readonly DotnetRunner Runner = new(TimeSpan.FromSeconds(120));

    private static Exercise Fixture(string name) =>
        Exercise.Parse(Fixtures.Path("running"),
            Fixtures.Path($"running/exercises/00_run/{name}"));

    // A broken exercise with full metadata is exactly what we want to ship.
    [Fact]
    public async Task Broken_exercise_with_metadata_is_ok()
    {
        int exit = await VerifyBroken.RunAsync([Fixture("002_wrong_output.cs")], Runner);
        Assert.Equal(0, exit);
    }

    // An exercise that already passes teaches nothing; the gate must fail.
    [Fact]
    public async Task Passing_exercise_is_a_violation()
    {
        int exit = await VerifyBroken.RunAsync([Fixture("001_ok.cs")], Runner);
        Assert.Equal(1, exit);
    }
}
