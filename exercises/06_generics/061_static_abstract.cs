// [C# 11] Static abstract interface members
//
// EN: Interfaces have always been able to require *instance* members —
//     a method or property every implementing type provides its own
//     copy of, reachable only through an instance (`something.Member`).
//     There was no way to require a *type-level* member: something you
//     call on the type itself, with no instance in hand, like `T.Zero`
//     or `T.Parse(...)`. Generic math needs exactly that — a generic
//     method with `where T : INumber<T>` wants to write `T.Zero` inside
//     its body, and every numeric type plugged in as `T` must supply
//     it. C# 11's `static abstract` (and `static virtual`) members in
//     interfaces close that gap: an interface declares a member as
//     `static abstract`, each implementing type supplies a matching
//     `static` member, and a generic method constrained to that
//     interface can call `T.Member` directly. Unlike `virtual` or
//     `abstract` instance members there is no runtime dispatch here:
//     the compiler resolves the call at compile time, from the type
//     information the generic constraint provides — each closed `T`
//     ends up with a direct call to its own static member. This is the
//     exact mechanism the next exercise's `INumber<T>` is built on.
// JA: interface はこれまでも「インスタンスメンバー」— 実装する型ごとに
//     自分のコピーを持ち、インスタンス経由（`something.Member`）でしか
//     到達できないメソッドやプロパティ — を要求できました。しかし
//     「型レベルのメンバー」、つまりインスタンスを持たずに型そのものに
//     対して呼び出せるもの（`T.Zero` や `T.Parse(...)` のような）を要求
//     する方法はありませんでした。generic math はまさにそれを必要と
//     します。`where T : INumber<T>` というジェネリックメソッドは本体
//     の中で `T.Zero` と書きたく、`T` に当てはめられるすべての数値型が
//     それを提供しなければなりません。C# 11 の `static abstract`（と
//     `static virtual`）メンバーはその隙間を埋めます。interface は
//     メンバーを `static abstract` として宣言し、実装する型はそれぞれ
//     一致する `static` メンバーを提供します。すると、その interface で
//     制約されたジェネリックメソッドは `T.Member` を直接呼び出せます。
//     クラスの `virtual` / `abstract` インスタンスメンバーと違い、ここ
//     に実行時ディスパッチはありません。コンパイラはジェネリック制約が
//     与えるコンパイル時の型情報からこの呼び出しを解決し、閉じられた
//     `T` ごとに、その型自身の static メンバーへの直接呼び出しになり
//     ます。これはまさに、次の exercise で登場する `INumber<T>` が
//     構築されている仕組みそのものです。
//
// Unity note:
// EN: Mono — Unity's scripting runtime through 6.7 — has no runtime
//     support for `static abstract` interface members at all: a struct
//     or class implementing them compiles and runs fine on .NET 10
//     here, but the same code fails outright on Mono (and today's
//     IL2CPP). The feature only becomes usable once Unity ships its
//     CoreCLR-based runtime in 6.8. See docs/feature-matrix.md for
//     exactly which Unity version / runtime tier this needs.
// JA: Unity のスクリプティングランタイムである Mono（6.7 まで）は、
//     `static abstract` interface メンバーのランタイムサポートをまった
//     く持っていません。それらを実装した struct や class は、ここ
//     （.NET 10）ではコンパイル・実行できますが、同じコードは Mono
//     （そして現行の IL2CPP）ではまったく動きません。この機能が使える
//     ようになるのは、Unity が CoreCLR ベースのランタイムを 6.8 で出荷
//     してからです。どの Unity バージョン／ランタイム階層で必要になる
//     かは docs/feature-matrix.md を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface#static-abstract-and-virtual-members

static TSelf Merge<TSelf>(TSelf a, TSelf b) where TSelf : IMeter<TSelf> =>
    TSelf.Combine(a, b);

StaminaPoints total = Merge(
    new StaminaPoints { Value = 30 },
    new StaminaPoints { Value = 15 });

Console.WriteLine(total.Value);

interface IMeter<TSelf> where TSelf : IMeter<TSelf>
{
    static abstract TSelf Zero { get; }
    static abstract TSelf Combine(TSelf a, TSelf b);
}

struct StaminaPoints : IMeter<StaminaPoints>
{
    public int Value { get; init; }

    public static StaminaPoints Zero => new() { Value = 0 };
}

// HINT EN: The compile error names an interface member that
//          StaminaPoints never supplies. A `static abstract` member is
//          satisfied the same way `Zero` already is — look at how that
//          property fulfills its declaration, then do the same for the
//          missing member. EXPECTED OUTPUT tells you what merging the
//          two meters above must produce.
// HINT JA: コンパイルエラーは、StaminaPoints が一度も提供していない
//          interface メンバーの名前を挙げています。`static abstract`
//          メンバーの満たし方は、`Zero` がすでにやっているのと同じで
//          す。そのプロパティが宣言をどう満たしているかを見て、不足し
//          ているメンバーにも同じことをしてください。上の 2 つのメー
//          ターを統合した結果がどうなるべきかは EXPECTED OUTPUT が
//          教えてくれます。
//
// EXPECTED OUTPUT:
// 45
