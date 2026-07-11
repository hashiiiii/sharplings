using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ExerciseLocatorTests
{
    // Zero-padded numeric prefixes make ordinal string order the intended
    // course order; chapters sort before later chapters, files within them.
    [Fact]
    public void Discovers_exercises_in_ordinal_path_order()
    {
        var exercises = ExerciseLocator.Discover(Fixtures.Path("locating"));

        Assert.Equal(
            [
                Path.Combine("exercises", "00_alpha", "001_a.cs"),
                Path.Combine("exercises", "00_alpha", "002_b.cs"),
                Path.Combine("exercises", "01_beta", "011_c.cs"),
            ],
            exercises.Select(e => e.RelativePath).ToArray());
    }

    // The runner is started from arbitrary directories inside the repo;
    // the root is wherever the exercises/ directory lives.
    [Fact]
    public void Finds_root_from_nested_directory()
    {
        string nested = Fixtures.Path("locating/exercises/01_beta");
        Assert.Equal(Fixtures.Path("locating"), ExerciseLocator.FindRoot(nested));
    }

    [Fact]
    public void Throws_when_no_exercises_directory_above()
    {
        Assert.Throws<InvalidOperationException>(
            () => ExerciseLocator.FindRoot(Path.GetTempPath()));
    }
}
