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

    private static Dictionary<string, string> Load(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? []
            : [];

    private static string HashOf(Exercise exercise) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(exercise.AbsolutePath)));
}
