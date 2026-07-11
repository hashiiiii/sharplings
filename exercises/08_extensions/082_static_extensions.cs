// [C# 14] Static extension members
//
// EN: An old-style extension method always extends *instances* — its
//     first parameter is `this SomeType value`, and the value being
//     extended has to already exist before you can call anything on
//     it. There was never a way to add a *static* member — a
//     `Type.Something` you can call without an instance in hand, like
//     a factory — to a type you don't own; that always had to live in
//     its own separately-named helper class (`IntParsing.ParseOrZero`,
//     not `int.ParseOrZero`). C# 14's `extension` block lifts this
//     restriction: writing the receiver's *type* with no parameter
//     name, `extension(int) { ... }`, declares members that attach to
//     `int` itself rather than to an `int` value. A `public static int
//     ParseOrZero(string text) => ...` inside that block is called as
//     `int.ParseOrZero("42")` — syntactically indistinguishable from a
//     "real" static factory method that `System.Int32` shipped with.
// JA: 従来スタイルの拡張メソッドは常に *インスタンス* を拡張します —
//     最初の引数は `this SomeType value` で、何かを呼び出す前に拡張
//     対象の値がすでに存在していなければなりません。インスタンスを
//     手にせず呼べる `Type.何か` のような *static* メンバー（ファクトリ
//     のようなもの）を、ソースを持たない型に追加する方法は今まで
//     ありませんでした。それは常に、別名を持つヘルパークラス
//     （`int.ParseOrZero` ではなく `IntParsing.ParseOrZero`）に置く
//     しかありませんでした。C# 14 の `extension` block はこの制約を
//     取り払います。レシーバーの *型* だけを引数名なしで書く
//     `extension(int) { ... }` は、`int` の値ではなく `int` 型自体に
//     結び付くメンバーを宣言します。この block の中の
//     `public static int ParseOrZero(string text) => ...` は
//     `int.ParseOrZero("42")` として呼び出され、構文上は
//     `System.Int32` が最初から持っていた「本物の」static ファクトリ
//     メソッドと見分けが付きません。
//
// Unity note:
// EN: The C# 9-era answer to "give me a safe parse with a fallback" was
//     always a static helper class — `Parsing.IntOrZero(text)`,
//     `SaveDataUtils.ParseHp(text)` — one per project, named slightly
//     differently every time, and never discoverable from the type
//     itself. Static extension members put the factory back on the
//     type it's conceptually about: `int.ParseOrZero(text)` reads at
//     the call site exactly like `int.Parse(text)` already does. Like
//     extension properties in the previous exercise, this is pure
//     compile-time sugar — no runtime dependency, so Mono/IL2CPP is not
//     the blocker. What gates it is the Unity-bundled compiler's
//     support for C# 14; check docs/feature-matrix.md for the version
//     where that lands.
// JA: 「フォールバック付きの安全な parse が欲しい」という C# 9 時代の
//     答えは、常に static なヘルパークラスでした — `Parsing.IntOrZero
//     (text)`、`SaveDataUtils.ParseHp(text)` のように、プロジェクトごと
//     に少しずつ違う名前が付き、その型自体からは決して見つけられません
//     でした。static extension member は、そのファクトリを概念上それが
//     属する型の上に戻します。`int.ParseOrZero(text)` は呼び出し箇所で
//     既存の `int.Parse(text)` と全く同じように読めます。前の exercise
//     の extension property と同様、これも純粋なコンパイル時の糖衣構文
//     です — ランタイム依存はなく、Mono / IL2CPP がブロッカーには
//     なりません。関門になるのは Unity にバンドルされたコンパイラが
//     C# 14 に対応しているかどうかです。どのバージョンで対応するかは
//     docs/feature-matrix.md を確認してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#extension-members

Console.WriteLine(int.ParseOrZero("42"));
Console.WriteLine(int.ParseOrZero("not a number"));

static class IntExtensions
{
    ???
    {
        public static int ParseOrZero(string text) =>
            int.TryParse(text, out int value) ? value : 0;
    }
}

// HINT EN: `ParseOrZero` right below is `static`, so it attaches to the
//          type `int` itself, not to an `int` value — the extension
//          block header names only the receiver *type*, with no
//          parameter name (unlike the instance form `extension(Vector2
//          v)` from the previous exercise). Replace `???` with
//          `extension(int)`.
// HINT JA: すぐ下の `ParseOrZero` は `static` なので、`int` の値では
//          なく `int` 型自体に結び付きます — extension block の
//          ヘッダーはレシーバーの *型* だけを名付け、引数名を持ちません
//          （前の exercise のインスタンス形式 `extension(Vector2 v)`
//          とは異なります）。`???` を `extension(int)` に置き換えて
//          ください。
//
// EXPECTED OUTPUT:
// 42
// 0
