// [C# 11] Span pattern matching against constant strings
//
// EN: A `ReadOnlySpan<char>` can be compared directly against a
//     string-literal pattern in `is` or `switch` — `span is "quit"` —
//     with no allocation. The compiler lowers this to an efficient
//     span comparison. Before C# 11 you'd need `span.SequenceEqual(
//     "quit")`, or convert the span to a string first with
//     `.ToString()`, which allocates a new string on every call.
// JA: `ReadOnlySpan<char>` は `is` や `switch` の中で、文字列リテラルの
//     パターンと直接比較できます — `span is "quit"` — しかも
//     アロケーションはありません。コンパイラはこれを効率的な span の
//     比較に変換します。C# 11 より前は `span.SequenceEqual("quit")`
//     を使うか、`.ToString()` で先に文字列へ変換する必要があり、
//     後者は呼び出すたびに新しい文字列をアロケーションします。
//
// Unity note:
// EN: Think of parsing a debug/cheat console's input line: you slice
//     a `ReadOnlySpan<char>` out of the raw input buffer and dispatch
//     on the token — one token per keystroke, potentially many times
//     a frame — without allocating a new string for every comparison.
//     This is the zero-alloc parsing story C# 11 was built for.
// JA: デバッグ用・チート用コンソールの入力行を解析する場面を考えて
//     ください。生の入力バッファから `ReadOnlySpan<char>` を
//     切り出し、そのトークンで分岐します — 1 キー入力につき 1
//     トークン、1 フレームに何度も発生し得ますが、比較のたびに新しい
//     文字列をアロケーションすることはありません。これこそ C# 11 が
//     目指した、ゼロアロケーションのパース処理です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#pattern-match-spanchar-or-readonlyspanchar-on-a-constant-string

ReadOnlySpan<char> quit = "quit";
ReadOnlySpan<char> help = "help";
ReadOnlySpan<char> teleport = "teleport";

Console.WriteLine(Dispatch(quit));
Console.WriteLine(Dispatch(help));
Console.WriteLine(Dispatch(teleport));

static string Dispatch(ReadOnlySpan<char> command) => command switch
{
    ??? => "shutting down",
    "help" => "showing help",
    _ => "unknown command"
};

// HINT EN: ??? is the exact string literal this arm should match —
//          the same word used to declare `quit` above. C# 11 lets a
//          ReadOnlySpan<char> be compared straight against a
//          string-literal pattern, no `.ToString()` or
//          `.SequenceEqual` needed.
// HINT JA: ??? は、この arm がマッチすべき正確な文字列リテラルです —
//          上で `quit` を宣言したときと同じ単語です。C# 11 では
//          ReadOnlySpan<char> を文字列リテラルのパターンと直接
//          比較でき、`.ToString()` も `.SequenceEqual` も不要です。
//
// EXPECTED OUTPUT:
// shutting down
// showing help
// unknown command
