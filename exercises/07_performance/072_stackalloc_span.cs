// [C# 7.3] stackalloc into Span<T>
//
// EN: A small scratch buffer that lives for a single method call —
//     a temporary array of ints or floats to sort, accumulate into, or
//     hand to a helper — was almost always `new int[n]` in C# 9-era
//     Unity code: a heap allocation that becomes garbage the instant
//     the method returns. If that method runs every frame (a raycast
//     hit-buffer, a small sort inside a physics query), that is a
//     steady stream of Gen0 garbage. `stackalloc` itself predates C#
//     7.3, but until then it could only produce a raw pointer
//     (`int*`) inside an `unsafe` block — usable, but off-limits in
//     ordinary managed code. C# 7.3 lets `stackalloc` initialize a
//     `Span<T>` directly, in ordinary safe code: the memory lives on
//     the current method's stack frame, is reclaimed the instant the
//     method returns — exactly like any other local — and never
//     touches the GC-managed heap at all.
// JA: 1 回のメソッド呼び出しの間だけ生きる小さなスクラッチバッファ —
//     ソートしたり、積算したり、ヘルパーに渡したりする一時的な int
//     や float の配列 — は、C# 9 時代の Unity コードではほぼ常に
//     `new int[n]` でした。これはヒープアロケーションであり、メソッド
//     が戻った瞬間にゴミになります。そのメソッドが毎フレーム実行される
//     もの（レイキャストのヒットバッファ、物理クエリ内の小さなソート
//     など）なら、これは Gen0 ガベージの絶え間ない流れになります。
//     `stackalloc` 自体は C# 7.3 より前から存在しますが、それまでは
//     `unsafe` ブロックの中で生の ポインタ（`int*`）しか作れませんでした
//     — 使えはしますが、通常のマネージドコードでは使えません。C# 7.3
//     は `stackalloc` に、通常の safe なコードのまま `Span<T>` を直接
//     初期化させてくれます。そのメモリは現在のメソッドのスタック
//     フレーム上に存在し、他のローカル変数とまったく同じように、
//     メソッドが戻った瞬間に回収されます — GC が管理するヒープには
//     一切触れません。
//
// Unity note:
// EN: This is exactly the buffer shape Unity code wants inside
//     `Update()` or `OnCollisionEnter` — small, short-lived, allocated
//     fresh on every call — replaced by a stack-allocated `Span<T>`
//     that costs zero GC pressure no matter how many times per second
//     it runs. Like `AsSpan`/`Slice` in the previous exercise,
//     `stackalloc` into `Span<T>` already works on Unity's current
//     Mono/IL2CPP runtime today; nothing here is gated on the 6.8
//     transition.
// JA: これはまさに、Unity のコードが `Update()` や
//     `OnCollisionEnter` の中で欲しがるバッファの形です — 小さく、
//     短命で、呼び出すたびに新しく確保されるもの — それがスタック
//     に確保された `Span<T>` に置き換わり、1 秒間に何回実行されようと
//     GC 負荷はゼロになります。前の exercise の `AsSpan` / `Slice` と
//     同様、`stackalloc` から `Span<T>` を作るこの機能も、Unity の
//     現行の Mono / IL2CPP ランタイムですでに動きます。ここには 6.8
//     への移行を待つものは何もありません。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc

Span<int> buffer = ???;

for (int i = 0; i < buffer.Length; i++)
    buffer[i] = i * i;

int total = 0;
foreach (int value in buffer)
    total += value;

Console.WriteLine(total);

// HINT EN: The loop right below fills five slots (`i` from 0 to 4), so
//          the buffer needs exactly five `int`s of stack space. Replace
//          ??? with a `stackalloc` expression of that element type and
//          length — no `unsafe` keyword needed.
// HINT JA: すぐ下のループは 5 つのスロットを埋めます（`i` は 0 から
//          4 まで）。つまりバッファには `int` 5 つ分のスタック領域が
//          ちょうど必要です。??? を、その要素型と長さを持つ
//          `stackalloc` 式に置き換えてください — `unsafe` キーワードは
//          不要です。
//
// EXPECTED OUTPUT:
// 30
