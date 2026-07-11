// [C# 11] Generic math with INumber<T>
//
// EN: C# 9 had no way to write one generic numeric algorithm that
//     worked across `int`, `float`, `decimal`, and friends — a generic
//     constraint could bound a type parameter to an interface or base
//     class, but no interface existed that `int`, `float`, and
//     `decimal` all implemented and that exposed things like `Zero` or
//     the `+` operator. In practice this meant writing (or overloading)
//     the same "sum a sequence" helper once per numeric type. .NET 7
//     ships a family of "generic math" interfaces built on C# 11's
//     static abstract members — `INumber<T>` is the umbrella one,
//     requiring, among others, a static `Zero` property and support for
//     the arithmetic operators. A single `Sum<T>` constrained to
//     `where T : INumber<T>` now compiles once and works for every type
//     that implements it.
// JA: C# 9 には、`int`、`float`、`decimal` などをまたいで動く 1 つの
//     ジェネリックな数値アルゴリズムを書く方法がありませんでした。
//     ジェネリック制約は型引数を interface やベースクラスに束縛でき
//     ますが、`int`、`float`、`decimal` がすべて実装していて `Zero` や
//     `+` 演算子のようなものを公開する interface が存在しなかったから
//     です。実際には、同じ「シーケンスを合計する」ヘルパーを数値型ごと
//     に書く（あるいはオーバーロードする）ことになっていました。
//     .NET 7 では、C# 11 の static abstract メンバーの上に構築された
//     「generic math」interface 群が提供されています。`INumber<T>` は
//     その総称的な interface で、静的な `Zero` プロパティや算術演算子の
//     サポートなどを要求します。`where T : INumber<T>` で制約された
//     1 つの `Sum<T>` は、これを実装するすべての型に対して 1 度書くだけ
//     で動作するようになりました。
//
// Unity note:
// EN: Unity code today still carries a small zoo of per-type math
//     helpers — `SumInts`, `SumFloats`, a `decimal` variant for
//     currency — because Mono cannot run code built on `static
//     abstract` interface members (the mechanism the previous exercise
//     introduced): `INumber<T>` and the rest of generic math need that
//     mechanism to dispatch `T.Zero` and the operators. The feature
//     only becomes usable once Unity ships its CoreCLR-based runtime in
//     6.8; see docs/feature-matrix.md for exactly which version / tier.
// JA: 今日の Unity コードには、`SumInts`、`SumFloats`、通貨計算用の
//     `decimal` 版といった、型ごとの数値計算ヘルパーが今も小さな動物園
//     のように残っています。Mono は `static abstract` interface メン
//     バー（前の exercise で紹介した仕組み）の上に構築されたコードを
//     実行できないためです。`INumber<T>` をはじめとする generic math
//     は、`T.Zero` や各種演算子をディスパッチするためにまさにこの仕組
//     みを必要とします。この機能が使えるようになるのは、Unity が
//     CoreCLR ベースのランタイムを 6.8 で出荷してからです。どのバー
//     ジョン／階層で必要になるかは docs/feature-matrix.md を参照して
//     ください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/api/system.numerics.inumber-1

using System.Numerics;

static T Sum<T>(IEnumerable<T> values) where T : ???
{
    T total = T.Zero;
    foreach (T value in values)
        total += value;
    return total;
}

Console.WriteLine(Sum(new[] { 1, 2, 3, 4 }));
Console.WriteLine(Sum(new[] { 1f, 2f, 3f, 4f }));
Console.WriteLine(Sum(new[] { 10m, 20m, 30m }));

// HINT EN: Sum<T> already calls the static `T.Zero` and accumulates
//          with `+=` — both come from one umbrella generic-math
//          interface in `System.Numerics` that `int`, `float`, and
//          `decimal` all implement. Replace ??? with that interface.
// HINT JA: Sum<T> はすでに静的な `T.Zero` を呼び出し、`+=` で積算して
//          います — どちらも `System.Numerics` にある 1 つの総称的な
//          generic-math interface に由来し、`int`、`float`、`decimal`
//          はすべてそれを実装しています。??? をその interface に
//          置き換えてください。
//
// EXPECTED OUTPUT:
// 10
// 10
// 60
