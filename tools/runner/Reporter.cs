namespace Sharplings.Runner;

/// <summary>Console presentation. Kept free of logic so the loop stays testable by exit code.</summary>
public static class Reporter
{
    public static void PrintPass(Exercise exercise, bool cached)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  ✓ ");
        Console.ResetColor();
        Console.WriteLine(cached ? $"{exercise.RelativePath} (cached)" : exercise.RelativePath);
    }

    public static void PrintFailure(Exercise exercise, RunResult result, int passed, int total)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  ✗ ");
        Console.ResetColor();
        Console.WriteLine(exercise.RelativePath);
        Console.WriteLine();
        Console.WriteLine(result.Verdict switch
        {
            Verdict.CompileError => "The exercise does not compile:",
            Verdict.RuntimeError => "The exercise crashed while running:",
            Verdict.OutputMismatch => "The exercise runs, but its output does not match:",
            Verdict.Timeout => "The exercise did not finish in time (infinite loop?):",
            _ => result.Verdict.ToString(),
        });
        Console.WriteLine();

        if (result.Verdict is Verdict.OutputMismatch)
        {
            Console.WriteLine("--- expected ---------------------------------");
            Console.WriteLine(exercise.ExpectedOutput);
            Console.WriteLine("--- actual -----------------------------------");
            Console.WriteLine(result.Stdout.TrimEnd());
            Console.WriteLine("----------------------------------------------");
        }
        else
        {
            if (result.Stdout.Trim().Length > 0) Console.WriteLine(result.Stdout.TrimEnd());
            if (result.Stderr.Trim().Length > 0) Console.WriteLine(result.Stderr.TrimEnd());
        }

        Console.WriteLine();
        foreach (string hint in exercise.Hints)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(hint);
            Console.ResetColor();
        }
        Console.WriteLine();
        Console.WriteLine($"Progress: {passed}/{total}");
        Console.WriteLine($"Edit {exercise.RelativePath} and run the runner again.");
    }

    public static void PrintAllPassed(int total)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"All {total} exercises pass. Modern C# is yours.");
        Console.ResetColor();
    }
}
