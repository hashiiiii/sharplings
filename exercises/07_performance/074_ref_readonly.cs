// [C# 12] ref readonly parameters
//
// EN: Passing a large struct by value copies every field onto the
//     callee's stack on every single call — for a Unity-sized value
//     type (a `Matrix4x4` is 64 bytes; a custom transform-snapshot or
//     physics-state struct is often bigger), that copy happens on
//     every call, all day, every frame. C# 7.2's `in` modifier passes
//     the argument by reference instead — no copy — while still
//     stopping the callee from writing back through it. The trouble
//     with `in` is how quiet it is: a caller can write `Total(stats)`
//     with no modifier at all, or even hand it a temporary value with
//     no variable behind it, and the compiler happily copies it in for
//     you — so nothing at the call site tells you whether a copy
//     actually happened. `ref readonly` (C# 12) keeps the exact same
//     "read through a reference, never write through it" contract as
//     `in`, but the compiler now warns when a caller doesn't pass an
//     addressable variable explicitly with `ref` or `in` — so a
//     `ref readonly` parameter announces, much more loudly than `in`
//     ever did, "I need a reference to your existing variable, not a
//     copy of a value."
// JA: 大きな struct を値渡しすると、呼び出しのたびにすべてのフィールド
//     が呼び出し先のスタックにコピーされます — Unity サイズの value
//     type（`Matrix4x4` は 64 バイト、独自の transform スナップショット
//     や物理状態の struct はもっと大きいことがよくあります）では、この
//     コピーが毎フレーム、あらゆる呼び出しで発生します。C# 7.2 の `in`
//     修飾子は、コピーせずに引数を参照渡しする一方で、呼び出し先が
//     それを通じて書き戻すことは防ぎます。`in` の問題は、その静かさ
//     です — 呼び出し側は修飾子なしで `Total(stats)` と書くことも、
//     背後に変数を持たない一時的な値をそのまま渡すこともでき、
//     コンパイラは喜んでそれをコピーして渡してくれます。つまり呼び出し
//     箇所を見ても、実際にコピーが起きたかどうかは何もわかりません。
//     `ref readonly`（C# 12）は `in` とまったく同じ「参照経由で読む
//     だけで、決して書き戻さない」という契約を維持しますが、呼び出し
//     側が `ref` や `in` で明示的にアドレス指定可能な変数を渡さない
//     場合、コンパイラが警告するようになります。つまり `ref readonly`
//     パラメーターは、`in` よりもずっとはっきりと「値のコピーではなく、
//     あなたの既存の変数への参照が必要だ」と表明します。
//
// Unity note:
// EN: This one is not a Mono-versus-CoreCLR story at all — `ref
//     readonly` is purely a C# 12 compiler rule (the marker the
//     compiler needs is generated into your own assembly automatically,
//     the same way `in` already compiles today), so it works on
//     Unity's current scripting runtime the moment Unity's bundled
//     compiler accepts C# 12 syntax. The payoff is API design, not
//     runtime availability: once your team writes physics or transform
//     helpers that take a big struct by `in`, `ref readonly` lets you
//     say "do not defensive-copy this, and do not expect to write
//     through it either" as a compiler-checked contract instead of a
//     comment above the method.
// JA: これは Mono か CoreCLR かという話では全くありません —
//     `ref readonly` は純粋に C# 12 のコンパイラ規則であり
//     （コンパイラが必要とするマーカーは、今日 `in` がすでに
//     コンパイルされているのと同じように、自分のアセンブリに自動的に
//     生成されます）、Unity にバンドルされたコンパイラが C# 12 の構文
//     を受け付けるようになれば、Unity の現行のスクリプティング
//     ランタイム上でそのまま動きます。ここでの利点はランタイムの
//     対応状況ではなく API 設計です。チームが大きな struct を `in`
//     で受け取る物理演算や transform 用のヘルパーを書くようになったら、
//     `ref readonly` はメソッドの上のコメントの代わりに、コンパイラが
//     検証する契約として「これを防御的にコピーするな、書き戻しも
//     期待するな」と言えるようになります。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#ref-readonly-parameters

BigStats stats = new BigStats { A = 10, B = 20, C = 30, D = 40 };

Console.WriteLine(Total(in stats));

static long Total(??? BigStats stats) => stats.A + stats.B + stats.C + stats.D;

struct BigStats
{
    public long A, B, C, D;
}

// HINT EN: The call site above already passes `in stats` — a caller
//          ready for the stricter contract. The parameter itself still
//          needs the two-word C# 12 modifier that matches that
//          contract: read-only access through a reference, not a copy,
//          and not a plain `in`.
// HINT JA: 上の呼び出し箇所はすでに `in stats` を渡しています —
//          より厳格な契約を受け入れる準備ができた呼び出し側です。
//          パラメーター自体には、その契約に合う C# 12 の 2 単語の
//          修飾子がまだ必要です — 参照経由の読み取り専用アクセスで
//          あり、コピーではなく、また単なる `in` でもありません。
//
// EXPECTED OUTPUT:
// 100
