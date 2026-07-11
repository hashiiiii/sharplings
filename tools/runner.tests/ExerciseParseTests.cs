using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ExerciseParseTests
{
    // The parser must extract the title line, the bilingual hint blocks
    // (with continuation lines), and the expected-output block.
    [Fact]
    public void Parses_title_expected_output_and_bilingual_hints()
    {
        string root = Fixtures.Path("parsing");
        var exercise = Exercise.Parse(root,
            Fixtures.Path("parsing/exercises/00_parse/001_sample.cs"));

        Assert.Equal("[C# 12] Sample exercise", exercise.Title);
        Assert.Equal("line one\nline two", exercise.ExpectedOutput);
        Assert.Equal(2, exercise.Hints.Count);
        Assert.StartsWith("HINT EN: First hint line.", exercise.Hints[0]);
        Assert.Contains("Continuation of the English hint.", exercise.Hints[0]);
        Assert.StartsWith("HINT JA:", exercise.Hints[1]);
    }

    // RelativePath is relative to the repo root so cache keys and display
    // stay stable regardless of where the runner is invoked from.
    [Fact]
    public void Relative_path_is_rooted_at_repo_root()
    {
        string root = Fixtures.Path("parsing");
        var exercise = Exercise.Parse(root,
            Fixtures.Path("parsing/exercises/00_parse/001_sample.cs"));

        Assert.Equal(
            Path.Combine("exercises", "00_parse", "001_sample.cs"),
            exercise.RelativePath);
    }
}
