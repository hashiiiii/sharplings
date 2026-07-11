// [C# 11] Generic attributes
//
// EN: Before C# 11, an attribute class could never be generic —
//     `class HandlerAttribute<TEvent> : Attribute` was flatly illegal,
//     because a generic type could not derive from the special
//     `Attribute` base class at all. Anything that wanted to attach a
//     *type* to a declaration via an attribute had to accept a plain
//     `System.Type` parameter instead — `[Handler(typeof(PlayerDied))]`
//     — which throws away all compile-time checking of what that type
//     actually is: a caller could pass `typeof(int)` and the compiler
//     would never object. C# 11 lifts the restriction: an attribute
//     class can now be generic, as long as it is closed (fully
//     specified) everywhere it is applied — `[Handler<PlayerDied>]` is
//     legal, `[Handler<T>]` inside another generic type is not.
//     Reflection can then read the type argument back off the applied
//     attribute exactly like any other generic type argument.
// JA: C# 11 より前は、属性クラスをジェネリックにすることは決してでき
//     ませんでした。`class HandlerAttribute<TEvent> : Attribute` は
//     完全に不正でした。ジェネリック型は特別なベースクラスである
//     `Attribute` を継承すること自体ができなかったからです。属性を
//     使って宣言に「型」を添付したい場合は、代わりに素の `System.Type`
//     引数を受け取るしかありませんでした —
//     `[Handler(typeof(PlayerDied))]` — これはその型が実際に何である
//     かのコンパイル時チェックを完全に放棄してしまいます。呼び出し側が
//     `typeof(int)` を渡してもコンパイラは決して文句を言いません。C# 11
//     はこの制限を撤廃しました。属性クラスは、適用されるすべての箇所で
//     閉じている（型引数が完全に確定している）限り、ジェネリックにでき
//     ます。`[Handler<PlayerDied>]` は合法ですが、別のジェネリック型の
//     内側での `[Handler<T>]` は合法ではありません。リフレクションは、
//     他のジェネリック型引数とまったく同じように、適用された属性から
//     型引数を読み戻せます。
//
// Unity note:
// EN: Generic attributes are a pure C# compiler feature with no special
//     runtime support required just to declare or apply them — but
//     Unity's Mono / IL2CPP reflection stack has historically crashed
//     or misbehaved when *reflecting over* a generic attribute at
//     runtime (walking `GetCustomAttributes` on a type decorated with
//     one), which is exactly what this exercise's `DescribeHandler`
//     does. Treat generic-attribute reflection as 6.8/CoreCLR territory
//     in Unity code until you have checked docs/feature-matrix.md for
//     your target version.
// JA: ジェネリック属性そのものを宣言・適用するだけなら、特別なランタイ
//     ムサポートを必要としない純粋な C# コンパイラの機能です。しかし
//     Unity の Mono / IL2CPP のリフレクションスタックは、実行時にジェネ
//     リック属性を「リフレクションで読み取る」際（そのような属性が付い
//     た型に対して `GetCustomAttributes` を辿る際）に、歴史的にクラッ
//     シュしたり誤動作したりしてきました。まさにこの exercise の
//     `DescribeHandler` が行っていることです。対象の Unity バージョン
//     を docs/feature-matrix.md で確認するまでは、ジェネリック属性の
//     リフレクションは 6.8 / CoreCLR の領域だと考えてください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/generics-and-attributes

Console.WriteLine(DescribeHandler(typeof(PlayerDiedHandler)));
Console.WriteLine(DescribeHandler(typeof(GameOverHandler)));

static string DescribeHandler(Type handlerType)
{
    object attribute = handlerType.GetCustomAttributes(inherit: false).Single();
    Type eventType = attribute.GetType().GetGenericArguments()[0];
    return $"{handlerType.Name} handles {eventType.Name}";
}

class HandlerAttribute<TEvent> : ??? { }

class PlayerDied { }
class GameOver { }

[Handler<PlayerDied>]
class PlayerDiedHandler { }

[Handler<GameOver>]
class GameOverHandler { }

// HINT EN: C# 11's generic-attribute rule is specifically about what a
//          generic attribute class is allowed to derive from — replace
//          ??? with the base class every attribute class derives from,
//          generic or not.
// HINT JA: C# 11 のジェネリック属性のルールは、ジェネリックな属性クラス
//          が何を継承してよいかについてのものです。??? を、ジェネリッ
//          クであるかどうかに関わらずすべての属性クラスが継承する
//          ベースクラスに置き換えてください。
//
// EXPECTED OUTPUT:
// PlayerDiedHandler handles PlayerDied
// GameOverHandler handles GameOver
