using Sharplings.Runner;

namespace Sharplings.Runner.Tests;

public class ProgressCacheTests
{
    // Build a tiny throwaway exercise in a temp dir; the cache only reads
    // file bytes, so the content does not need to compile.
    private static (Exercise Exercise, string Root) MakeExercise(string content)
    {
        string root = Directory.CreateTempSubdirectory("sharplings-cache-").FullName;
        string dir = Path.Combine(root, "exercises", "00_t");
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, "001_t.cs");
        File.WriteAllText(file, content);
        return (Exercise.Parse(root, file), root);
    }

    [Fact]
    public void Unknown_exercise_is_not_passed()
    {
        var (exercise, root) = MakeExercise("// v1");
        var cache = new ProgressCache(Path.Combine(root, "cache.json"));
        Assert.False(cache.IsPassed(exercise));
    }

    [Fact]
    public void Marked_exercise_is_passed_and_survives_reload()
    {
        var (exercise, root) = MakeExercise("// v1");
        string cachePath = Path.Combine(root, "cache.json");
        new ProgressCache(cachePath).MarkPassed(exercise);

        // A fresh instance re-reads the JSON file from disk.
        Assert.True(new ProgressCache(cachePath).IsPassed(exercise));
    }

    [Fact]
    public void Editing_the_file_invalidates_the_pass()
    {
        var (exercise, root) = MakeExercise("// v1");
        string cachePath = Path.Combine(root, "cache.json");
        var cache = new ProgressCache(cachePath);
        cache.MarkPassed(exercise);

        File.WriteAllText(exercise.AbsolutePath, "// v2");
        Assert.False(cache.IsPassed(exercise));
    }

    [Fact]
    public void Corrupted_cache_file_is_treated_as_empty()
    {
        // File.WriteAllText is not atomic, so a Ctrl-C mid-write (or a
        // crash) can leave .sharplings-cache.json truncated. The cache
        // should heal itself instead of crashing every future run.
        var (exercise, root) = MakeExercise("// v1");
        string cachePath = Path.Combine(root, "cache.json");
        File.WriteAllText(cachePath, "{ \"half");

        var cache = new ProgressCache(cachePath);
        Assert.False(cache.IsPassed(exercise));

        // Writing through the healed cache should work and produce a
        // valid file again.
        cache.MarkPassed(exercise);
        Assert.True(new ProgressCache(cachePath).IsPassed(exercise));
    }
}
