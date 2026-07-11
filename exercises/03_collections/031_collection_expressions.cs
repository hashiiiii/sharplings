// [C# 12] Collection expressions
//
// EN: C# 12 introduces collection expressions: a single `[...]` literal
//     that builds whatever collection type the surrounding code expects.
//     `int[] fibonacci = [1, 1, 2, 3, 5];` replaces the old
//     `new int[] { 1, 1, 2, 3, 5 };`, and the very same bracket syntax
//     also builds a `List<string>` two lines below — the compiler picks
//     the right construction from the variable's declared type, not
//     from anything inside the brackets. The `..` (spread) operator on
//     the last line unpacks an existing collection's elements into a
//     new literal; you will use it to merge multiple sources together
//     in the next exercise.
// JA: C# 12 では collection expression が導入されました。周囲のコードが
//     求めるコレクション型を、1 つの `[...]` リテラルで構築します。
//     `int[] fibonacci = [1, 1, 2, 3, 5];` は従来の
//     `new int[] { 1, 1, 2, 3, 5 };` を置き換え、同じ角括弧の構文が
//     その 2 行下で `List<string>` も構築します。どちらの構築方法を
//     使うかは角括弧の中身ではなく、変数側で宣言された型から決まり
//     ます。最後の行にある `..`（spread）演算子は、既存のコレクション
//     の要素を新しいリテラルへ展開するもので、複数の集合をまとめる
//     使い方は次の exercise で扱います。
//
// Unity note:
// EN: Unity code is full of hand-built waypoint arrays and loot tables:
//     `new Transform[] { a, b, c }`, `new List<LootEntry> { entry1,
//     entry2 }`. Collection expressions shrink both to `[a, b, c]` and
//     `[entry1, entry2]` — same allocation, less punctuation, and one
//     consistent literal shape whether the field is an array, a
//     `List<T>`, or (soon) a `Span<T>`.
// JA: Unity のコードには手作りの waypoint 配列や loot table があふれて
//     います。`new Transform[] { a, b, c }` や
//     `new List<LootEntry> { entry1, entry2 }` などです。collection
//     expression を使うとどちらも `[a, b, c]` / `[entry1, entry2]`
//     に縮まり、確保されるものは同じままで記号が減り、フィールドが
//     配列でも `List<T>` でも（後述する `Span<T>` でも）同じ形の
//     リテラルで書けるようになります。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions

int[] fibonacci = ???;                       // was: new int[] { 1, 1, 2, 3, 5 }
List<string> party = [???, "Ryu", "Ken"];    // one member is missing
int[] merged = [..fibonacci, ..fibonacci];

Console.WriteLine(string.Join(' ', fibonacci));
Console.WriteLine(string.Join(' ', party));
Console.WriteLine(merged.Length);

// HINT EN: Replace the first `???` with a bracketed literal holding the
//          five Fibonacci values named in the trailing comment. Replace
//          the second `???` with the missing third party member's name
//          — the roster is meant to read "Chun-Li Ryu Ken".
// HINT JA: 1 つ目の `???` を、末尾のコメントに書かれている 5 つの
//          フィボナッチ数を並べた角括弧リテラルに置き換えてください。
//          2 つ目の `???` には欠けている 3 人目のパーティメンバー名を
//          入れます。ロースターは "Chun-Li Ryu Ken" になるはずです。
//
// EXPECTED OUTPUT:
// 1 1 2 3 5
// Chun-Li Ryu Ken
// 10
