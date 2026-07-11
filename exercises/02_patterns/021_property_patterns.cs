// [C# 10] Extended property patterns
//
// EN: C# 9 already let you match a nested property, but only by
//     nesting braces yourself: `enemy is { Stats: { Hp: > 0 } }`.
//     C# 10 adds extended property patterns — write the dotted path
//     straight through instead: `enemy is { Stats.Hp: > 0 }`. Same
//     check, same compiled result, no inner braces to balance.
// JA: C# 9 でもネストしたプロパティを照合できましたが、`enemy is
//     { Stats: { Hp: > 0 } }` のように波括弧を自分でネストする必要が
//     ありました。C# 10 では拡張プロパティパターンが追加され、ドット
//     区切りのパスをそのまま書けます: `enemy is { Stats.Hp: > 0 }`。
//     チェック内容もコンパイル結果も同じですが、内側の波括弧を揃える
//     必要がありません。
//
// Unity note:
// EN: Unity code often guards on a nested stat this way:
//     `if (enemy.Stats.Hp > 0) { ... }` on a plain data hierarchy
//     (not a GetComponent lookup — this is C# 9's nested-pattern
//     territory, not Unity's component system). Before C# 10, writing
//     that same check as a pattern needed its own nested
//     `{ Stats: { Hp: > 0 } }` brace pair; the dotted form now matches
//     how you already read the member-access chain.
// JA: Unity のコードでは、ネストした値をこのように守ることがよく
//     あります: `if (enemy.Stats.Hp > 0) { ... }`（これは GetComponent
//     ではなく、プレーンなデータ階層に対する話です — C# 9 のネストした
//     パターンの領域です）。C# 10 より前は、同じチェックをパターンとして
//     書くには `{ Stats: { Hp: > 0 } }` という専用の入れ子の波括弧が
//     必要でした。ドット形式なら、すでに読み慣れたメンバーアクセスの
//     並びのまま書けます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#extended-property-patterns

var ongoing = new Battle(new Fighter("Goblin", 15));
var finished = new Battle(new Fighter("Goblin", 0));

Console.WriteLine(Status(ongoing));
Console.WriteLine(Status(finished));

static string Status(Battle b) => b switch
{
    { ??? } => "Enemy still standing",
    _ => "Enemy defeated"
};

record Fighter(string Name, int Hp);
record Battle(Fighter Enemy);

// HINT EN: ??? is one extended property pattern:
//          <OuterProperty>.<InnerProperty>: <relational pattern>,
//          matching Battle's Enemy property, then reaching straight
//          into its Hp. It should hold when Hp is greater than zero.
// HINT JA: ??? は 1 つの拡張プロパティパターンです:
//          <外側のプロパティ>.<内側のプロパティ>: <関係パターン>
//          という形で、Battle の Enemy プロパティから、その Hp まで
//          一直線にアクセスします。Hp が 0 より大きいときに成立する
//          ようにしてください。
//
// EXPECTED OUTPUT:
// Enemy still standing
// Enemy defeated
