namespace Sharplings.Runner;

/// <summary>Quality gate: every committed exercise must be broken and fully annotated.</summary>
public static class VerifyBroken
{
    public static async Task<int> RunAsync(
        IReadOnlyList<Exercise> exercises, DotnetRunner runner)
    {
        List<string> violations = [];
        foreach (Exercise exercise in exercises)
        {
            if (exercise.ExpectedOutput.Length == 0)
                violations.Add($"{exercise.RelativePath}: missing EXPECTED OUTPUT");
            if (!exercise.Hints.Any(static h => h.StartsWith("HINT EN")))
                violations.Add($"{exercise.RelativePath}: missing HINT EN");
            if (!exercise.Hints.Any(static h => h.StartsWith("HINT JA")))
                violations.Add($"{exercise.RelativePath}: missing HINT JA");

            RunResult result = await runner.RunAsync(exercise);
            if (result.Verdict is Verdict.Pass)
                violations.Add($"{exercise.RelativePath}: already passes (not broken)");
            Console.WriteLine($"checked {exercise.RelativePath}: {result.Verdict}");
        }

        foreach (string violation in violations)
            Console.Error.WriteLine($"VIOLATION: {violation}");
        Console.WriteLine($"{exercises.Count} exercise(s) checked, {violations.Count} violation(s)");
        return violations.Count == 0 ? 0 : 1;
    }
}
