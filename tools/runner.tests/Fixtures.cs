namespace Sharplings.Runner.Tests;

public static class Fixtures
{
    public static string Path(string relative) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "fixtures",
            relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
}
