using System.Security.Cryptography;
using System.Text.Json;

namespace Sharplings.Runner;

/// <summary>Remembers which exercises passed, keyed by content hash so any edit re-runs the file.</summary>
public sealed class ProgressCache(string cachePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly Dictionary<string, string> _passed = Load(cachePath);

    public bool IsPassed(Exercise exercise) =>
        _passed.TryGetValue(exercise.RelativePath, out string? hash)
        && hash == HashOf(exercise);

    public void MarkPassed(Exercise exercise)
    {
        _passed[exercise.RelativePath] = HashOf(exercise);
        File.WriteAllText(cachePath, JsonSerializer.Serialize(_passed, JsonOptions));
    }

    private static Dictionary<string, string> Load(string path)
    {
        if (!File.Exists(path))
            return [];

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? [];
        }
        catch (System.Text.Json.JsonException)
        {
            // File.WriteAllText is not atomic: a Ctrl-C (or crash) mid-write
            // can leave a truncated/corrupted cache file behind. Treat that
            // as an empty cache instead of crashing every future run — the
            // next MarkPassed call rewrites a valid file.
            return [];
        }
    }

    private static string HashOf(Exercise exercise) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(exercise.AbsolutePath)));
}
