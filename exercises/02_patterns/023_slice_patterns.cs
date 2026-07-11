// [C# 11] Slice patterns
//
// EN: A list pattern can also capture, not just match. Give the
//     two-dot range pattern a variable designation and it captures
//     every element it covers into a new array: `[var first, .. var
//     rest]` binds the first element to `first` and everything after
//     it, as its own array, to `rest`. This is a slice pattern —
//     the same `..` used in 022, but now with a name attached. Before
//     C# 11 you'd slice by hand: `var first = queue[0]; var rest =
//     queue[1..];`.
// JA: list パターンは照合するだけでなく、値を取り込むこともできます。
//     2 つのドットの range パターンに designation（変数名）を付けると、
//     それがカバーするすべての要素を新しい配列として取り込みます。
//     `[var first, .. var rest]` は先頭の要素を `first` に、それ以降の
//     すべてを配列として `rest` に束縛します。これは slice パターンで、
//     022 で使った `..` と同じ記号に名前を付けたものです。C# 11 より
//     前は、これを手動でスライスしていました:
//     `var first = queue[0]; var rest = queue[1..];`。
//
// Unity note:
// EN: A spawn/patrol queue is a natural fit: the first entry is the
//     active target, the rest are waiting their turn. Before C# 11,
//     splitting the queue meant indexing `queue[0]` and range-slicing
//     `queue[1..]` as two separate steps; the slice pattern does both
//     in the same match.
// JA: 敵の出現順やパトロールの待機列は、まさにこの形にぴったりです。
//     先頭の要素が今アクティブな対象で、残りは自分の番を待っています。
//     C# 11 より前は、待機列を分けるのに `queue[0]` のインデックス
//     アクセスと `queue[1..]` の range スライスを別々の 2 手順で
//     書く必要がありました。slice パターンなら、同じ 1 回のマッチで
//     両方を済ませられます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#list-patterns

string[] squad = ["Goblin", "Orc", "Troll"];
string[] lone = ["Slime"];

Report(squad);
Report(lone);

static void Report(string[] queue)
{
    var (first, remaining) = Split(queue);
    Console.WriteLine(remaining.Length == 0
        ? $"engaging {first} alone"
        : $"engaging {first}, {remaining.Length} more waiting");
}

static (string First, string[] Remaining) Split(string[] queue) => queue switch
{
    [var first, ???] => (first, rest),
    [] => ("none", [])
};

// HINT EN: ??? is a slice pattern: two dots followed by a variable
//          designation that captures every remaining element as its
//          own array. Name it `rest` — that name is already used in
//          the arm's result tuple, right after `first`.
// HINT JA: ??? は slice パターンです。2 つのドットのあとに、残りの
//          要素すべてを 1 つの配列として取り込む変数の designation を
//          続けます。名前は `rest` にしてください — この名前は
//          `first` のすぐ後ろ、arm の結果 tuple の中ですでに使われて
//          います。
//
// EXPECTED OUTPUT:
// engaging Goblin, 2 more waiting
// engaging Slime alone
