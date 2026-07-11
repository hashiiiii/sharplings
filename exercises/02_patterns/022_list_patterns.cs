// [C# 11] List patterns
//
// EN: A list pattern matches an array (or anything countable and
//     indexable) by its shape, written with square brackets: `[]`
//     matches an empty sequence, `[x]` matches exactly one element and
//     captures it into `x`. Two dots, `..`, stand for "any number of
//     further elements here" — so `[1, 2, ..]` matches any sequence
//     that starts with 1 then 2, no matter how much follows. Before
//     C# 11 you'd write this by hand: checking `.Length` and indexing,
//     e.g. `buffer.Length >= 2 && buffer[0] == 1 && buffer[1] == 2`.
// JA: list パターンは、配列（や数えられてインデックスアクセスできる
//     もの）を「形」で照合します。角括弧で書き、`[]` は空のシーケンス、
//     `[x]` はちょうど 1 要素にマッチしてそれを `x` に取り込みます。
//     2 つのドット `..` は「この位置に任意個の残り要素がある」ことを
//     表します。つまり `[1, 2, ..]` は、後に何が続いても 1、2 の順で
//     始まるすべてのシーケンスにマッチします。C# 11 より前は、これを
//     `.Length` とインデックスで手書きしていました。例:
//     `buffer.Length >= 2 && buffer[0] == 1 && buffer[1] == 2`。
//
// Unity note:
// EN: Picture an input-combo buffer for a fighting-game character
//     controller: an `int[]` of recent button codes. Before C# 11,
//     recognizing "starts with 1, 2" meant the `.Length` and index
//     checks above, repeated for every move in the list. A switch
//     expression with list patterns reads like the move list itself.
// JA: 格闘ゲーム風のキャラクターコントローラーの、入力コンボバッファを
//     想像してください。直近のボタンコードを並べた `int[]` です。
//     C# 11 より前は「1、2 の順で始まる」ことの判定に、上記の `.Length`
//     とインデックスのチェックが必要で、これを技の数だけ繰り返して
//     書くことになります。list パターンを使った switch 式なら、
//     技一覧そのものを読んでいるような書き方になります。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#list-patterns

int[] idle = [];
int[] jab = [7];
int[] special = [1, 2, 3];

Console.WriteLine(Recognize(idle));
Console.WriteLine(Recognize(jab));
Console.WriteLine(Recognize(special));

static string Recognize(int[] buffer) => buffer switch
{
    [] => "idle",
    ??? => "special move",
    [var single] => $"jab ({single})",
    _ => "unknown input"
};

// HINT EN: ??? is a list pattern matching any buffer that starts with
//          the button codes 1 then 2, followed by any number of
//          further inputs. Write the two literal numbers, then the
//          two-dot range pattern meaning "and anything after."
// HINT JA: ??? は、ボタンコード 1、2 の順で始まり、その後に任意個の
//          入力が続くバッファすべてにマッチする list パターンです。
//          2 つのリテラルの数値を書いたあとに、「この後に何があっても
//          よい」を表す 2 つのドットの range パターンを続けてください。
//
// EXPECTED OUTPUT:
// idle
// jab (7)
// special move
