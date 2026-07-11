// [C# 13] allows ref struct
//
// EN: A `ref struct` (the family `Span<T>` belongs to) must never
//     leave the stack — it cannot be boxed, stored in a field of a
//     reference type, or captured by a lambda. Before C# 13 that rule
//     had a side effect: a `ref struct` could not implement an
//     interface at all, because calling a member through an interface
//     reference risked boxing the value underneath it — exactly what
//     a `ref struct` forbids. That left `Span<T>`-backed types with no
//     way to satisfy any interface-based abstraction; helpers had to
//     be duplicated by hand, one concrete overload per ref-struct-like
//     type. C# 13 closes this gap with two features working together.
//     First, a `ref struct` may now implement an interface, as long as
//     it is never used in a way that would force boxing. Second, a
//     generic type parameter must now opt in, with an explicit
//     `allows ref struct` anti-constraint, before a `ref struct` can be
//     substituted for it at all — without that opt-in, the compiler
//     still refuses a `ref struct` type argument exactly as it always
//     has, protecting every generic method written before C# 13 that
//     assumes its `T` can be boxed or stored on the heap.
// JA: `ref struct`（`Span<T>` が属する系統）は決してスタックから
//     出てはいけません — ボックス化することも、参照型のフィールドに
//     格納することも、ラムダにキャプチャされることもできません。
//     C# 13 より前、このルールには副作用がありました。`ref struct`
//     は interface を実装することが一切できなかったのです。interface
//     の参照経由でメンバーを呼び出すと、その裏の値がボックス化されて
//     しまう恐れがあり、それはまさに `ref struct` が禁じていることだ
//     からです。そのため `Span<T>` を背後に持つ型には、interface
//     ベースの抽象化を満たす手段が一切なく、ヘルパーは ref-struct 的な
//     型ごとに具象のオーバーロードを 1 つずつ手作業で複製するしか
//     ありませんでした。C# 13 は 2 つの機能を組み合わせてこの隙間を
//     埋めます。1 つ目、`ref struct` はボックス化を強制しない使い方に
//     限り、interface を実装できるようになりました。2 つ目、ジェネリック
//     な型引数は、明示的な `allows ref struct` アンチ制約でオプトイン
//     しない限り、そもそも `ref struct` をそこに当てはめることが
//     できません — このオプトインがなければ、コンパイラは今まで
//     どおり `ref struct` 型引数を拒否し続け、C# 13 より前に書かれた、
//     `T` がボックス化やヒープ格納できることを前提とするすべての
//     ジェネリックメソッドを守ります。
//
// Unity note:
// EN: Ref struct interfaces and the `allows ref struct` anti-constraint
//     both depend on the runtime being able to instantiate generic
//     code over a `ref struct` type argument with no boxing at all —
//     something Mono's generics implementation (Unity's scripting
//     backend through 6.7) has no support for. Code built on either
//     feature fails outright on today's Unity, not just "runs slower."
//     Both become usable once Unity ships its CoreCLR-based runtime in
//     6.8; see docs/feature-matrix.md for exactly which Unity version /
//     runtime tier this needs.
// JA: ref struct の interface 実装と `allows ref struct` アンチ制約は
//     どちらも、`ref struct` 型引数に対してボックス化なしにジェネリック
//     コードをインスタンス化できるランタイムの能力に依存しています —
//     これは Mono のジェネリクス実装（6.7 までの Unity のスクリプティング
//     バックエンド）がまったくサポートしていないものです。どちらかの
//     機能の上に構築されたコードは、今日の Unity では「遅くなる」
//     どころか、そのまま動きません。両方とも、Unity が CoreCLR ベース
//     のランタイムを 6.8 で出荷すれば使えるようになります。どの Unity
//     バージョン／ランタイム階層で必要になるかは docs/feature-matrix.md
//     を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#allows-ref-struct

int[] rawScores = [10, 20, 30, 40, 50];
ScoreBuffer buffer = new ScoreBuffer(rawScores);

Console.WriteLine(Describe(buffer));

static int Describe<T>(T counter) where T : ICounter
    => counter.Count;

interface ICounter
{
    int Count { get; }
}

ref struct ScoreBuffer : ICounter
{
    private readonly Span<int> _scores;

    public ScoreBuffer(Span<int> scores) => _scores = scores;

    public int Count => _scores.Length;
}

// HINT EN: `ScoreBuffer` is a `ref struct` implementing `ICounter`,
//          and `Describe<T>` calls it through `where T : ICounter`
//          alone — the compiler error names the exact anti-constraint
//          `T` is missing before a `ref struct` type argument is
//          allowed. Add it to the `where` clause, after `ICounter`.
// HINT JA: `ScoreBuffer` は `ICounter` を実装した `ref struct` で、
//          `Describe<T>` は `where T : ICounter` だけを通してそれを
//          呼び出しています — コンパイルエラーは、`ref struct` 型引数
//          が許される前に `T` に不足している、まさにそのアンチ制約の
//          名前を挙げています。`where` 句に、`ICounter` の後ろに
//          それを追加してください。
//
// EXPECTED OUTPUT:
// 5
