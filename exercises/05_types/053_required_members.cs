// [C# 11] Required members
//
// EN: An object initializer has always been able to skip properties —
//     that's the point of making them optional. The trouble is a
//     property that must always be set couldn't be marked that way:
//     the only way to force it was a constructor parameter, and once
//     several properties needed to be mandatory while others stayed
//     optional, you ended up hand-writing (or overloading) a
//     constructor just to keep that guarantee, losing the readable,
//     named-argument feel of `new Foo { A = ..., B = ... }` in the
//     process. C# 11's `required` modifier marks a property (or field)
//     as mandatory: the compiler rejects any object initializer, or
//     any constructor not marked `[SetsRequiredMembers]`, that leaves
//     it unset. Callers keep writing plain object initializers; the
//     compiler enforces that the required ones are never skipped.
// JA: object initializer は元々プロパティを省略できました — それこそ
//     が「省略可能」であることの意味です。問題は、必ず設定してほしい
//     プロパティをそのようにマークする方法がなかったことです。強制す
//     る唯一の手段はコンストラクタの引数にすることでしたが、必須にし
//     たいプロパティと省略可能なままにしたいプロパティが混在すると、
//     その保証のためだけにコンストラクタを手書き（あるいはオーバー
//     ロード）することになり、`new Foo { A = ..., B = ... }` という
//     読みやすい名前付き引数のような書き方が失われてしまいます。C# 11
//     の `required` 修飾子はプロパティ（またはフィールド）を必須として
//     マークします。コンパイラは、それを設定しない object initializer
//     や `[SetsRequiredMembers]` の付いていないコンストラクタを拒否し
//     ます。呼び出し側は普通の object initializer を書き続けられ、必須
//     項目が省略されないことはコンパイラが保証します。
//
// Unity note:
// EN: Unity's Mono / IL2CPP scripting runtime has historically shipped
//     an older mscorlib that lacks the `RequiredMemberAttribute` and
//     `CompilerFeatureRequiredAttribute` marker types the compiler
//     emits for `required` — so pointing the compiler at a modern
//     `LangVersion` isn't enough by itself under Mono. The PolySharp
//     NuGet package polyfills those marker attributes into your own
//     assembly so `required` compiles and runs on the older runtime
//     tiers. See docs/feature-matrix.md for which Unity version /
//     runtime tier needs PolySharp versus which now ships the markers
//     natively (6.8+).
// JA: Unity の Mono / IL2CPP スクリプティングランタイムは、歴史的に
//     `required` のためにコンパイラが出力する `RequiredMemberAttribute`
//     と `CompilerFeatureRequiredAttribute` というマーカー型を持たない
//     古い mscorlib を使ってきました。そのため Mono では、コンパイラに
//     新しい `LangVersion` を指定するだけでは不十分です。PolySharp と
//     いう NuGet パッケージは、それらのマーカー属性を自分のアセンブリ
//     にポリフィルすることで、古いランタイム階層でも `required` をコン
//     パイル・実行できるようにします。どの Unity バージョン／ランタイム
//     階層に PolySharp が必要で、どこから標準でマーカーが提供されるか
//     （6.8 以降）は docs/feature-matrix.md を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required

var hero = new PlayerProfile
{
    Name = "Aria"
};

Console.WriteLine($"{hero.Name} Lv.{hero.Level}");

class PlayerProfile
{
    public required string Name { get; init; }
    public required int Level { get; init; }
}

// HINT EN: A new save profile always starts at Level 1 — check
//          EXPECTED OUTPUT below for the exact value, then add the
//          missing required member to the object initializer above.
// HINT JA: 新しいセーブプロファイルは常に Level 1 から始まります。
//          正確な値は下の EXPECTED OUTPUT を確認し、上の object
//          initializer に不足している required メンバーを追加して
//          ください。
//
// EXPECTED OUTPUT:
// Aria Lv.1
