// [C# 14] Extension blocks and properties
//
// EN: C# 9's only way to add behavior to a type you don't own — like
//     `System.Numerics.Vector2`, whose source you can't edit — was an
//     extension *method*: a static method in a static class with `this`
//     on its first parameter (`public static float Manhattan(this
//     Vector2 v) => ...`). It always reads like a method call,
//     `v.Manhattan()`; `v.Manhattan` with no parentheses was illegal, no
//     matter how property-like the computation felt. C# 14 adds an
//     `extension` block: inside a `static class`, `extension(Vector2 v)
//     { ... }` opens a scope where `v` names the receiver for every
//     member declared inside, and those members can be properties, not
//     only methods. `public float ManhattanLength => MathF.Abs(v.X) +
//     MathF.Abs(v.Y);` compiles to the same IL an old-style extension
//     method would, but callers write `position.ManhattanLength` with
//     no parentheses — from the caller's side it really is a property.
// JA: C# 9 で、ソースを編集できない型 — `System.Numerics.Vector2` の
//     ような型 — に振る舞いを追加する唯一の方法は拡張 *メソッド* でした。
//     最初の引数に `this` を付けた static class 内の static メソッドです
//     （`public static float Manhattan(this Vector2 v) => ...`）。これは
//     常にメソッド呼び出しのように見えます。`v.Manhattan()` です。計算
//     内容がどれほどプロパティらしくても、括弧を付けない `v.Manhattan`
//     は不正でした。C# 14 は `extension` block を追加します。
//     static class の中で `extension(Vector2 v) { ... }` と書くと、内部
//     で宣言するすべてのメンバーに対して `v` がレシーバーとして有効に
//     なるスコープが開き、そのメンバーはメソッドだけでなくプロパティ
//     にもできます。`public float ManhattanLength => MathF.Abs(v.X) +
//     MathF.Abs(v.Y);` は従来スタイルの拡張メソッドと同じ IL に
//     コンパイルされますが、呼び出し側は括弧なしで
//     `position.ManhattanLength` と書きます — 呼び出し側から見ると、
//     それは本当にプロパティなのです。
//
// Unity note:
// EN: Unity code leans hard on `Vector2`/`Vector3`, and a C# 9-era
//     project answers "I need one more computed fact about a vector"
//     with a static helper class — `VectorUtils.Manhattan(v)`,
//     `VectorUtils.IsAxisAligned(v)` — scattered across the codebase,
//     each call site one more place to remember the helper exists.
//     Extension properties let that computed fact live on the vector
//     itself: `v.ManhattanLength`, `v.IsAxisAligned`, discoverable by
//     typing `v.` and reading IntelliSense, exactly like a "real"
//     member of `Vector2` would be. This is compile-time sugar only —
//     it lowers to the same static-method IL Unity's Mono/IL2CPP
//     runtime has always executed, so nothing here needs the 6.8
//     CoreCLR runtime. The only gate is whether the Unity-bundled
//     compiler accepts C# 14 syntax at all; see docs/feature-matrix.md
//     for which Unity version's compiler does.
// JA: Unity のコードは `Vector2`/`Vector3` に強く依存しており、C# 9
//     時代のプロジェクトで「このベクトルについてもう 1 つ計算値が欲しい」
//     となると、答えは static なヘルパークラスでした —
//     `VectorUtils.Manhattan(v)`、`VectorUtils.IsAxisAligned(v)` が
//     コードベースのあちこちに散らばり、呼び出し箇所ごとにそのヘルパー
//     の存在を覚えておく必要がありました。extension property を使えば、
//     その計算値をベクトル自身に持たせられます。`v.ManhattanLength`、
//     `v.IsAxisAligned` は `v.` と打って IntelliSense を読むだけで
//     見つかり、まるで `Vector2` の「本物の」メンバーであるかのように
//     使えます。これはコンパイル時の糖衣構文にすぎません — Unity の
//     Mono / IL2CPP ランタイムが以前から実行してきたのと同じ static
//     メソッドの IL に落ちるため、ここには 6.8 の CoreCLR ランタイムは
//     必要ありません。唯一の関門は Unity にバンドルされたコンパイラが
//     C# 14 構文を受け付けるかどうかです。どの Unity バージョンの
//     コンパイラが対応しているかは docs/feature-matrix.md を参照して
//     ください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension

using System.Numerics;

Vector2 position = new Vector2(3, 4);
Vector2 onAxis = new Vector2(0, 5);

Console.WriteLine((int)position.ManhattanLength);
Console.WriteLine(position.IsAxisAligned);
Console.WriteLine(onAxis.IsAxisAligned);

static class Vector2Extensions
{
    ???
    {
        public float ManhattanLength => MathF.Abs(v.X) + MathF.Abs(v.Y);

        public bool IsAxisAligned => v.X == 0 || v.Y == 0;
    }
}

// HINT EN: Both members below already use `v` as the receiver — `v.X`,
//          `v.Y`. Replace `???` with the extension block header that
//          names `v` as a `Vector2` receiver:
//          `extension(Vector2 v)`.
// HINT JA: 下の両方のメンバーはすでに `v` をレシーバーとして使って
//          います — `v.X`、`v.Y` です。`???` を、`v` を `Vector2`
//          のレシーバーとして名付ける extension block のヘッダーに
//          置き換えてください。`extension(Vector2 v)` です。
//
// EXPECTED OUTPUT:
// 7
// False
// True
