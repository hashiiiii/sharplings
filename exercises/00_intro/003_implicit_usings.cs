// [C# 10] Implicit usings
//
// EN: Since the .NET 6 SDK (the C# 10 era), console/file-based project
//     templates enable ImplicitUsings: a handful of common namespaces
//     — System, System.Collections.Generic, System.Linq, System.IO,
//     System.Net.Http, System.Threading, System.Threading.Tasks — are
//     brought in for free, no `using` line required. That is why
//     `scores.Sum()` below just works. It does NOT cover every
//     namespace, though: System.Text (StringBuilder) still needs an
//     explicit `using`.
// JA: .NET 6 SDK（C# 10 の時代）以降、コンソール／file-based app の
//     プロジェクトテンプレートでは ImplicitUsings が有効になっており、
//     System、System.Collections.Generic、System.Linq、System.IO、
//     System.Net.Http、System.Threading、System.Threading.Tasks
//     といった代表的な名前空間は `using` を書かなくても使えます。下の
//     `scores.Sum()` が何もせずに動くのはこのためです。ただし全ての
//     名前空間を網羅しているわけではなく、System.Text（StringBuilder）
//     は今も明示的な `using` が必要です。
//
// Unity note:
// EN: Unity-generated .csproj files predate this SDK default and don't
//     enable it, which is why Unity scripts still spell out
//     `using System.Linq;` / `using System.Collections;` even though a
//     plain .NET 10 console app already gets those for free.
// JA: Unity が生成する .csproj はこの SDK のデフォルト設定より古く、
//     ImplicitUsings を有効にしていません。素の .NET 10 コンソールアプリ
//     では不要な `using System.Linq;` / `using System.Collections;` を
//     Unity のスクリプトでは今も書く必要があるのはこのためです。
//
// Docs: https://learn.microsoft.com/dotnet/core/project-sdk/overview#implicit-using-directives

int[] scores = { 95, 88, 100 };
int total = scores.Sum();

var report = new StringBuilder();
report.Append("Total score: ");
report.Append(total);
Console.WriteLine(report.ToString());

// HINT EN: `Sum()` works with no `using` because System.Linq is one of
//          the implicit usings. StringBuilder lives in System.Text,
//          which is not on that list — add the missing `using` line.
// HINT JA: `Sum()` が `using` なしで動くのは System.Linq が implicit
//          usings に含まれるためです。StringBuilder は System.Text に
//          あり、これは一覧に含まれていません。足りない using 行を
//          追加してください。
//
// EXPECTED OUTPUT:
// Total score: 283
