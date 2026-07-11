// [C# 12] Alias any type, including tuples
//
// EN: A `using` alias directive has always let you give a named type a
//     shorter name: `using Health = System.Int32;`. Before C# 12, that
//     was the limit — you could alias a named type, but not a tuple,
//     an array, a pointer, or any other unnamed type. If you wanted a
//     friendly name for `(int Row, int Col)`, your only option was to
//     write a whole new type: a struct or record with `Row` and `Col`
//     properties, just to get a readable name in signatures. C# 12
//     removes that limit: `using GridCell = (int Row, int Col);` gives
//     a tuple shape its own alias, usable in every signature below,
//     with the two-line ceremony of a dedicated type gone.
// JA: `using` エイリアスディレクティブは、以前から名前付き型に短い名前
//     を与えられました: `using Health = System.Int32;`。しかし C# 12
//     より前はそこまでが限界でした — 名前付き型はエイリアスできても、
//     tuple や配列、ポインタなど名前を持たない型はエイリアスできません
//     でした。`(int Row, int Col)` に読みやすい名前を付けたければ、
//     `Row` と `Col` というプロパティを持つ struct や record を新しく
//     まるごと書くしか方法がありませんでした。シグネチャに読みやすい
//     名前を出すためだけに、です。C# 12 はこの制限を取り除きます:
//     `using GridCell = (int Row, int Col);` と書けば tuple の形その
//     ものに専用のエイリアスを与えられ、以下のすべてのシグネチャで使え
//     ます。専用の型を用意する二手間はもう不要です。
//
// Unity note:
// EN: Grid-based systems (tile maps, turn-based movement, pathfinding)
//     pass `(int, int)` coordinate pairs constantly, and Unity code has
//     traditionally reached for `Vector2Int` even when there's no
//     actual vector math involved, just because a bare tuple reads as
//     "two unrelated numbers" in a method signature. A tuple alias like
//     `GridCell` gives that pair a domain name — no vector-math API you
//     don't need, no throwaway struct to maintain.
// JA: グリッドベースのシステム（タイルマップ、ターン制の移動、経路探索
//     など）では `(int, int)` という座標ペアを常に受け渡しします。
//     Unity のコードは、ベクトル演算が実際には何も絡まない場面でも
//     慣習的に `Vector2Int` を使ってきました。ただの tuple はメソッド
//     シグネチャの中で「無関係な数値が 2 つ」に見えてしまうからです。
//     `GridCell` のような tuple エイリアスなら、必要のないベクトル演算
//     API を持ち込むことも、使い捨ての struct を保守することもなく、
//     そのペアにドメイン名を与えられます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#alias-any-type

using GridCell = ???;

GridCell start = (0, 0);
GridCell goal = (4, 3);

int ManhattanDistance(GridCell a, GridCell b) =>
    Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);

Console.WriteLine(ManhattanDistance(start, goal));

GridCell next = (start.Row + 1, start.Col + 1);
Console.WriteLine($"{next.Row}, {next.Col}");

// HINT EN: ??? is the tuple type this alias stands for. Look at how
//          `start`, `goal`, and `next` are constructed and how `.Row`
//          / `.Col` are used below to see the two named tuple elements
//          `GridCell` needs.
// HINT JA: ??? はこのエイリアスが指す tuple 型です。下で `start` /
//          `goal` / `next` がどう作られ、`.Row` / `.Col` がどう使わ
//          れているかを見て、`GridCell` に必要な 2 つの名前付き
//          tuple 要素を確認してください。
//
// EXPECTED OUTPUT:
// 7
// 1, 1
