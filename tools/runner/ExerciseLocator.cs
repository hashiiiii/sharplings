namespace Sharplings.Runner;

/// <summary>Finds the repo root and enumerates exercises in course order.</summary>
public static class ExerciseLocator
{
    public static string FindRoot(string startDirectory)
    {
        for (DirectoryInfo? dir = new(startDirectory); dir is not null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "exercises")))
                return dir.FullName;
        throw new InvalidOperationException(
            $"No 'exercises' directory found above '{startDirectory}'. " +
            "Run from inside the sharplings repo or pass --root <path>.");
    }

    public static IReadOnlyList<Exercise> Discover(string root) => [..
        Directory.EnumerateFiles(
                Path.Combine(root, "exercises"), "*.cs", SearchOption.AllDirectories)
            .OrderBy(static p => p, StringComparer.Ordinal)
            .Select(p => Exercise.Parse(root, p))];
}
