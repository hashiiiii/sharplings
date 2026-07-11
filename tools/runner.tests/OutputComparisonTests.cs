using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class OutputComparisonTests
{
    // Comparison must be forgiving about line endings, trailing spaces and
    // trailing blank lines, and strict about everything else.
    [Theory]
    [InlineData("a\nb", "a\nb", true)]
    [InlineData("a\r\nb\r\n", "a\nb", true)]
    [InlineData("a  \nb", "a\nb", true)]
    [InlineData("a\nb\n\n", "a\nb", true)]
    [InlineData("a\nc", "a\nb", false)]
    [InlineData("A\nb", "a\nb", false)]
    public void Compares_normalized_output(string actual, string expected, bool match) =>
        Assert.Equal(match, DotnetRunner.OutputMatches(actual, expected));
}
