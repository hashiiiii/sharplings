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
//     interface can call `T.Member` directly — the runtime dispatches
//     to whichever type `T` was closed with. This is the exact
//     mechanism the next exercise's `INumber<T>` is built on.
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
//     制約されたジェネリックメソッドは `T.Member` を直接呼び出せ、
//     実行時には `T` として閉じられた型の実装へディスパッチされます。
//     これはまさに、次の exercise で登場する `INumber<T>` が構築されて
//     いる仕組みそのものです。
//
// Unity note:
// EN: Mono — Unity's scripting runtime through 6.7 — has no support for
//     the virtual static interface dispatch this feature relies on: a
//     struct or class implementing `static abstract` members compiles
//     and runs fine on .NET 10 here, but the same code fails outright
//     on Mono (and today's IL2CPP). The feature only becomes usable
//     once Unity ships its CoreCLR-based runtime in 6.8. See
//     docs/feature-matrix.md for exactly which Unity version / runtime
//     tier this needs.
// JA: Unity のスクリプティングランタイムである Mono（6.7 まで）は、この
//     機能が依存する仮想的な静的 interface ディスパッチをまったく
//     サポートしていません。`static abstract` メンバーを実装した struct
//     や class は、ここ（.NET 10）ではコンパイル・実行できますが、同じ
//     コードは Mono（そして現行の IL2CPP）ではまったく動きません。この
//     機能が使えるようになるのは、Unity が CoreCLR ベースのランタイムを
//     6.8 で出荷してからです。どの Unity バージョン／ランタイム階層で
//     必要になるかは docs/feature-matrix.md を参照してください。
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

// HINT EN: StaminaPoints declares that it implements
//          IMeter<StaminaPoints> and supplies Zero, but it never
//          supplies a static Combine — that member is required too.
//          Add a `public static StaminaPoints Combine(StaminaPoints a,
//          StaminaPoints b)` to the struct, matching the interface's
//          signature, and have it add the two Value fields together.
// HINT JA: StaminaPoints は IMeter<StaminaPoints> を実装すると宣言し
//          Zero は提供していますが、static な Combine は一度も提供して
//          いません — こちらも必須のメンバーです。interface の
//          シグネチャに一致する `public static StaminaPoints
//          Combine(StaminaPoints a, StaminaPoints b)` を struct に
//          追加し、2 つの Value を足し合わせるようにしてください。
//
// EXPECTED OUTPUT:
// 45
