using Sharplings.Runner;

bool verifyBroken = false;
bool noCache = false;
string? rootOverride = null;
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--verify-broken": verifyBroken = true; break;
        case "--no-cache": noCache = true; break;
        case "--root" when i + 1 < args.Length: rootOverride = args[++i]; break;
        default:
            Console.Error.WriteLine(
                "usage: runner [--root <path>] [--no-cache] [--verify-broken]");
            return 2;
    }
}

string root = rootOverride is null
    ? ExerciseLocator.FindRoot(Environment.CurrentDirectory)
    : Path.GetFullPath(rootOverride);
IReadOnlyList<Exercise> exercises = ExerciseLocator.Discover(root);
var runner = new DotnetRunner(TimeSpan.FromSeconds(10));

if (verifyBroken)
    return await VerifyBroken.RunAsync(exercises, runner);

var cache = new ProgressCache(Path.Combine(root, ".sharplings-cache.json"));
int passed = 0;
foreach (Exercise exercise in exercises)
{
    if (!noCache && cache.IsPassed(exercise))
    {
        Reporter.PrintPass(exercise, cached: true);
        passed++;
        continue;
    }

    RunResult result = await runner.RunAsync(exercise);
    if (result.Verdict is Verdict.Pass)
    {
        cache.MarkPassed(exercise);
        Reporter.PrintPass(exercise, cached: false);
        passed++;
        continue;
    }

    Reporter.PrintFailure(exercise, result, passed, exercises.Count);
    return 1;
}

Reporter.PrintAllPassed(exercises.Count);
return 0;
