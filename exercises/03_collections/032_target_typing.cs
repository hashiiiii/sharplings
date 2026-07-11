// [C# 12] Target typing across containers
//
// EN: A bracket literal carries no type information of its own — the
//     compiler looks at the variable's declared type and constructs
//     whatever that type needs. The exact same `[1, 2, 3]` text below
//     becomes an `int[]`, a `List<int>`, a `Span<int>`, and an
//     `ImmutableArray<int>`, because each declaration on the left tells
//     the compiler which construction to emit. This is why collection
//     expressions read the same everywhere, unlike C# 9, where
//     `new int[] {...}`, `new List<int> {...}`, and
//     `ImmutableArray.Create(...)` were three unrelated syntaxes for
//     saying the same thing.
// JA: 角括弧リテラル自体は型情報を持ちません。コンパイラは変数側の
//     宣言された型を見て、その型が必要とする構築処理を生成します。
//     下のコードにある全く同じ `[1, 2, 3]` というテキストが、
//     `int[]`、`List<int>`、`Span<int>`、`ImmutableArray<int>` の
//     いずれにもなるのは、左辺の宣言がそれぞれ異なる構築方法を
//     コンパイラへ伝えているからです。これが collection expression が
//     どこでも同じ見た目になる理由です。C# 9 では `new int[] {...}`、
//     `new List<int> {...}`、そして `ImmutableArray.Create(...)` は、
//     同じことを言うための 3 つの無関係な構文でした。
//
// Unity note:
// EN: Swap the declared type and the same `[...]` still compiles: a
//     `Transform[]` waypoint array, a `List<Transform>` for a route
//     you'll edit at runtime, or a `Span<Transform>` for a per-frame
//     scratch buffer all accept the identical literal. You stop
//     choosing your initializer syntax based on the container and start
//     choosing the container based on what the code actually needs.
// JA: 宣言する型を入れ替えても同じ `[...]` がそのままコンパイルできます。
//     waypoint 用の `Transform[]`、実行時に編集するルート用の
//     `List<Transform>`、フレームごとのスクラッチバッファ用の
//     `Span<Transform>` — どれも同じリテラルを受け付けます。コンテナの
//     種類に合わせて初期化構文を選ぶ必要がなくなり、コードが本当に
//     必要とする性質に応じてコンテナを選べるようになります。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions#conversions

using System.Collections.Immutable;

int[] arr = [1, 2, 3];
List<int> list = [1, 2, 3];
Span<int> span = [1, 2, 3];
ImmutableArray<int> archive = ???;

Console.WriteLine(arr.Length);
Console.WriteLine(list.Count);
Console.WriteLine(span.Length);
Console.WriteLine(archive.Length);

// HINT EN: The three lines above `archive` all build from the same
//          `[1, 2, 3]` text — do the same for `ImmutableArray<int>`;
//          the target type on the left is all the compiler needs.
// HINT JA: `archive` より上の 3 行はすべて同じ `[1, 2, 3]` という
//          テキストから構築されています。`ImmutableArray<int>` でも
//          同じことをしてください。コンパイラが必要とするのは左辺の
//          ターゲット型だけです。
//
// EXPECTED OUTPUT:
// 3
// 3
// 3
// 3
