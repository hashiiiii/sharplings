// [C# 11] required and init together: who can set what, when
//
// EN: A `required init` property can be set in exactly two places.
//     The normal path is an object initializer at construction time —
//     `new Character { Health = 80 }` — and the compiler checks it:
//     leave `Health` out and you get a compile error (CS9035). The
//     other path is a constructor marked
//     `[SetsRequiredMembers]` (from `System.Diagnostics.CodeAnalysis`).
//     That attribute tells the compiler "trust this constructor to set
//     every required member itself," and switches off its usual check
//     for that one constructor — nothing stops the constructor from
//     simply forgetting a member. Either way, `init` still means what
//     it always means: once construction finishes, neither path lets
//     you reach back in and change `Health` again.
// JA: `required init` プロパティを設定できる場所はちょうど 2 つです。
//     通常の経路は構築時の object initializer — `new Character
//     { Health = 80 }` — で、これはコンパイラがチェックします。
//     `Health` を省略すればコンパイルエラー（CS9035）になります。もう
//     1 つの経路は `System.Diagnostics.CodeAnalysis` の
//     `[SetsRequiredMembers]` が付いたコンストラクタです。この属性は
//     「このコンストラクタが required メンバーをすべて自分で設定する
//     と信じてよい」とコンパイラに伝え、そのコンストラクタに対する
//     通常のチェックを無効にします — コンストラクタがうっかりメンバー
//     を 1 つ設定し忘れても、それを止めるものはありません。どちらの
//     経路でも `init` の意味そのものは変わりません。構築が終われば、
//     どちらの経路で作った場合でも `Health` を後から書き換えることは
//     できません。
//
// Unity note:
// EN: `[SetsRequiredMembers]` is exactly what you reach for when
//     bolting a convenience constructor onto a data class that already
//     uses `required` — a `FromSaveData(...)`-style constructor that
//     reconstructs a `PlayerProfile` from a legacy, serialization-
//     driven save format, for instance, where the caller has no
//     object-initializer syntax to work with at all.
// JA: `[SetsRequiredMembers]` は、すでに `required` を使っているデータ
//     クラスに便利なコンストラクタを追加したいときにまさに使うもので
//     す。たとえば、古いシリアライズ形式のセーブデータから
//     `PlayerProfile` を再構築する `FromSaveData(...)` のようなコンス
//     トラクタでは、呼び出し側はそもそも object initializer の構文を
//     使える立場にありません。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.setsrequiredmembersattribute

using System.Diagnostics.CodeAnalysis;

var aria = new Character { Name = "Aria", Health = 80 };
var rex = new Character("Rex");

Console.WriteLine($"{aria.Name}: {aria.Health}");
Console.WriteLine($"{rex.Name}: {rex.Health}");

class Character
{
    public required string Name { get; init; }
    public required int Health { get; init; }

    public Character() { }

    [SetsRequiredMembers]
    public Character(string name)
    {
        Name = name;
    }
}

// HINT EN: `[SetsRequiredMembers]` tells the compiler to trust this
//          constructor to set every required member itself, so it
//          skips the usual object-initializer check — nothing stops
//          the constructor from simply forgetting one. `Character(string
//          name)` never assigns `Health`, so `rex.Health` silently
//          stays at its type's default instead of the intended
//          starting value. Set it inside the constructor.
// HINT JA: `[SetsRequiredMembers]` は、このコンストラクタが required
//          メンバーをすべて自分で設定すると信じるようコンパイラに伝え
//          るため、通常の object initializer チェックが働きません —
//          コンストラクタがうっかり 1 つ設定し忘れても止まりません。
//          `Character(string name)` は `Health` を一度も設定していない
//          ため、`rex.Health` は意図した初期値ではなく型のデフォルト値
//          のままになります。コンストラクタの中で設定してください。
//
// EXPECTED OUTPUT:
// Aria: 80
// Rex: 100
