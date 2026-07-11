// [C# 14] First-class span conversions
//
// EN: `Span<T>` and `ReadOnlySpan<T>` have always had implicit
//     conversions from arrays (`int[]` converts to `Span<int>`) and
//     from `Span<T>` to `ReadOnlySpan<T>` — but before C# 14 those
//     conversions did not count when the compiler was deciding which
//     *extension method* a receiver expression could call. An
//     extension method declared with a span-typed receiver
//     (`this ReadOnlySpan<int> numbers`) could only be called on an
//     expression that already had exactly that type: an `int[]`
//     variable had to go through an explicit `.AsSpan()` call first —
//     `numbers.AsSpan().FastAverage()`, never `numbers.FastAverage()`
//     directly. C# 14 makes these span conversions "first-class": the
//     compiler now also allows them when matching an extension
//     method's receiver, so the same `int[]` or `Span<int>` variable
//     can call a `ReadOnlySpan<int>`-shaped extension method directly,
//     with no `.AsSpan()` step in between.
// JA: `Span<T>` と `ReadOnlySpan<T>` には、配列からの暗黙変換
//     （`int[]` は `Span<int>` に変換されます）や `Span<T>` から
//     `ReadOnlySpan<T>` への暗黙変換が以前から存在していました —
//     しかし C# 14 より前は、コンパイラがある receiver 式がどの
//     *拡張メソッド* を呼び出せるかを判断する際、これらの変換は
//     数に入りませんでした。span 型の receiver を持つ拡張メソッド
//     （`this ReadOnlySpan<int> numbers`）は、すでにちょうどその型を
//     持つ式に対してしか呼び出せませんでした。`int[]` 型の変数は、
//     まず明示的に `.AsSpan()` を呼ぶ必要がありました —
//     `numbers.AsSpan().FastAverage()` であり、
//     `numbers.FastAverage()` を直接呼ぶことは決してできませんでした。
//     C# 14 はこれらの span 変換を「第一級」にします。コンパイラは
//     拡張メソッドの receiver をマッチさせる際にもこれらの変換を許可
//     するようになったため、同じ `int[]` や `Span<int>` の変数が、
//     間に `.AsSpan()` を挟むことなく `ReadOnlySpan<int>` 型の拡張
//     メソッドを直接呼び出せます。
//
// Unity note:
// EN: This is a compiler/language-version story, a different axis
//     from the Mono-versus-CoreCLR runtime story elsewhere in this
//     chapter. First-class span conversions need nothing new from the
//     runtime — they are resolved entirely at compile time — so even
//     Unity's current Mono/IL2CPP runtime could run code built this
//     way. What actually gates it is Unity's own bundled script
//     compiler: its Roslyn version has to accept `LangVersion 14`
//     before this exact call shape (an extension method called
//     directly on an array or a `Span<T>`) is legal in a Unity
//     project, independent of whether that project's runtime is Mono
//     or 6.8's CoreCLR. Check the compiler-support column of
//     docs/feature-matrix.md for your Unity version before relying on
//     this.
// JA: これはコンパイラ／言語バージョンの話であり、この章の他の
//     ところにある Mono 対 CoreCLR というランタイムの話とは別の軸
//     です。first-class span conversion はランタイムから何も新しい
//     ものを必要としません — 完全にコンパイル時に解決されます — その
//     ため、Unity の現行の Mono / IL2CPP ランタイムでも、こうして
//     書かれたコードは実行できるはずです。実際にこれを制限しているのは
//     Unity 自身にバンドルされたスクリプトコンパイラです。プロジェクト
//     のランタイムが Mono か 6.8 の CoreCLR かに関わらず、この正確な
//     呼び出しの形（拡張メソッドを配列や `Span<T>` に対して直接呼び
//     出す）が Unity プロジェクトで合法になるには、その Roslyn
//     バージョンが `LangVersion 14` を受け付ける必要があります。これに
//     頼る前に、お使いの Unity バージョンについて docs/feature-matrix.md
//     のコンパイラ対応列を確認してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#implicit-span-conversions

int[] scores = [10, 20, 30, 40];
Span<int> liveBuffer = stackalloc int[] { 5, 15, 25 };

Console.WriteLine(scores.FastAverage());
Console.WriteLine(liveBuffer.FastAverage());

static class SpanMath
{
    public static int FastAverage(this int[] numbers)
    {
        int sum = 0;
        foreach (int n in numbers)
            sum += n;
        return numbers.Length == 0 ? 0 : sum / numbers.Length;
    }
}

// HINT EN: The failing call is the second one, on `liveBuffer` — a
//          `Span<int>` never converts to `int[]`, in any version of
//          C#, so `this int[] numbers` can never accept it. Do not
//          touch either call site or `liveBuffer`'s declaration; the
//          fix is entirely in `FastAverage`'s own parameter type. This
//          exercise's Docs link names the exact conversion C# 14 added
//          for extension-method receivers — retype the parameter to
//          the one span type that both an `int[]` and a `Span<int>`
//          convert to (the body already works unchanged for it).
// HINT JA: 失敗している呼び出しは 2 つ目、`liveBuffer` に対するもの
//          です — `Span<int>` はどの C# バージョンでも `int[]` には
//          決して変換されないため、`this int[] numbers` はそれを
//          絶対に受け取れません。どちらの呼び出し箇所も、
//          `liveBuffer` の宣言も変更しないでください。修正は
//          `FastAverage` 自身のパラメーターの型だけで完結します。
//          この exercise の Docs リンクは、C# 14 が拡張メソッドの
//          receiver のために追加した、まさにその変換を説明して
//          います。パラメーターを、`int[]` と `Span<int>` の両方が
//          変換できる 1 つの span 型に付け替えてください（本体は
//          そのままで動きます）。
//
// EXPECTED OUTPUT:
// 25
// 15
