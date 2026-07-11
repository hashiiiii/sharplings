namespace Sharplings.Runner;

/// <summary>One exercise file plus the metadata parsed from its comments.</summary>
public sealed record Exercise(
    string RelativePath,
    string AbsolutePath,
    string Title,
    string ExpectedOutput,
    IReadOnlyList<string> Hints)
{
    public static Exercise Parse(string root, string absolutePath)
    {
        string[] lines = File.ReadAllLines(absolutePath);

        string title = lines.FirstOrDefault(static l => l.StartsWith("// ["))
            is string titleLine
            ? titleLine[3..].Trim()
            : Path.GetFileName(absolutePath);

        List<string> expected = [];
        List<string> hints = [];
        int i = 0;
        while (i < lines.Length)
        {
            string line = lines[i].TrimEnd();
            if (line == "// EXPECTED OUTPUT:")
            {
                i++;
                while (i < lines.Length && lines[i].StartsWith("//"))
                    expected.Add(StripComment(lines[i++]));
            }
            else if (line.StartsWith("// HINT"))
            {
                List<string> parts = [StripComment(line)];
                i++;
                while (i < lines.Length
                       && lines[i].StartsWith("//")
                       && !lines[i].StartsWith("// HINT")
                       && lines[i].TrimEnd() != "// EXPECTED OUTPUT:")
                    parts.Add(StripComment(lines[i++]));
                hints.Add(string.Join('\n', parts).TrimEnd());
            }
            else
            {
                i++;
            }
        }

        return new Exercise(
            Path.GetRelativePath(root, absolutePath),
            absolutePath,
            title,
            string.Join('\n', expected),
            hints);
    }

    private static string StripComment(string line)
    {
        string s = line.TrimEnd();
        return s.StartsWith("// ") ? s[3..] : s.StartsWith("//") ? s[2..] : s;
    }
}
