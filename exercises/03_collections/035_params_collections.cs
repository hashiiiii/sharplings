// [C# 13] params collections
//
// EN: Since C# 2, `params T[] values` has let callers pass a comma-
//     separated argument list without building the array themselves —
//     but the compiler still builds that array, on the heap, on every
//     call. C# 13 extends `params` to other collection types, including
//     `ReadOnlySpan<T>`: `params ReadOnlySpan<int> values` accepts the
//     exact same call syntax (`Sum(1, 2, 3, 4)`), but for the common
//     case the compiler can hand the method a stack-allocated span
//     instead of a heap array — the calling convention you already
//     know, without the hidden allocation.
// JA: C# 2 以降、`params T[] values` は呼び出し側が配列を自分で組み立て
//     ることなくカンマ区切りの引数リストを渡せるようにしてきましたが、
//     コンパイラは呼び出しのたびにヒープ上へその配列を組み立てて
//     いました。C# 13 では `params` を他のコレクション型にも拡張して
//     おり、`ReadOnlySpan<T>` もその 1 つです。
//     `params ReadOnlySpan<int> values` はまったく同じ呼び出し構文
//     （`Sum(1, 2, 3, 4)`）を受け付けますが、多くの場合コンパイラは
//     ヒープ配列の代わりにスタック上に確保された span をメソッドへ
//     渡せます。すでに知っている呼び出し方のまま、隠れたアロケー
//     ションだけがなくなります。
//
// Unity note:
// EN: A per-frame helper — a damage-log formatter, a debug-draw call —
//     taking `params` arguments used to mean one throwaway array
//     allocation per call, every frame: exactly the kind of GC pressure
//     Unity profiling sessions chase down. `params ReadOnlySpan<T>`
//     keeps the convenient call site and removes the allocation.
// JA: ダメージログのフォーマッタや、`params` 引数を取るデバッグ描画
//     呼び出しのような毎フレーム実行されるヘルパーは、これまで呼び
//     出しごとに使い捨ての配列を 1 つアロケートしていました。これは
//     まさに Unity のプロファイリングで追いかけることになる GC 負荷
//     そのものです。`params ReadOnlySpan<T>` は便利な呼び出し方を
//     そのままに、そのアロケーションだけを取り除きます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-13.0/params-collections

static int Sum(???)
{
    int total = 0;
    foreach (var v in values)
        total += v;
    return total;
}

Console.WriteLine(Sum(1, 2, 3, 4));
Console.WriteLine(Sum());

// HINT EN: The method body already loops over a variable named
//          `values`, and it is called both with a comma-separated
//          argument list and with no arguments at all — that is the
//          `params` calling convention. Declare the parameter as a
//          `params ReadOnlySpan<int>` named `values`.
// HINT JA: メソッド本体はすでに `values` という名前の変数をループして
//          おり、カンマ区切りの引数リストでも引数なしでも呼び出されて
//          います — これが `params` の呼び出し方です。パラメーターを
//          `values` という名前の `params ReadOnlySpan<int>` として
//          宣言してください。
//
// EXPECTED OUTPUT:
// 10
// 0
