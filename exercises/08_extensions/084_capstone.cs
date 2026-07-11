// [C# 14] Capstone: a modernized inventory system
//
// EN: This is the last exercise of sharplings. No new syntax appears
//     below — instead, a small item-inventory system is written the
//     way a C# 9-era Unity project would write it, then modernized
//     end to end using four features from across this course: a
//     `record` instead of a hand-rolled data class, a property pattern
//     inside a `switch` expression instead of an `if`/`else if` ladder,
//     a collection expression instead of `new Item[] { ... }`, and an
//     extension property instead of a separate static helper method.
//     None of these four ideas is new by itself — you've solved all of
//     them already, in earlier chapters — what's new is seeing them
//     compose in one realistic piece of code, each pulling its own
//     weight without the others getting in the way.
// JA: これは sharplings の最後の exercise です。以下に新しい構文は
//     登場しません — 代わりに、C# 9 時代の Unity プロジェクトが書く
//     ような小さなアイテムインベントリシステムを書き、このコース全体
//     から集めた 4 つの機能で端から端まで現代化します。手作りのデータ
//     クラスの代わりに `record`、`if`/`else if` の連鎖の代わりに
//     `switch` 式内のプロパティパターン、`new Item[] { ... }` の代わり
//     に collection expression、そして別立ての static ヘルパーメソッド
//     の代わりに extension property です。この 4 つの考え方はどれも
//     単体では新しくありません — すでに以前の章ですべて解いています —
//     新しいのは、それらが 1 つの現実的なコードの中で組み合わさる様子
//     を見ることです。互いの邪魔をせず、それぞれが自分の役割を果たし
//     ます。
//
// Unity note:
// EN: An item/inventory system — loot tables, shop stock, save-data
//     rosters — is one of the most common pieces of gameplay code in
//     any Unity project, and it tends to accumulate exactly the C# 9
//     habits modernized below: a mutable class for item data, a long
//     `if`/`else if` tier classifier, `new List<Item> { ... }` built up
//     with `.Add()` calls, and a `static class ItemUtils` for anything
//     computed from an item's fields. Everything below is compile-time
//     sugar — records, patterns, collection expressions, and extension
//     members all compile down to ordinary IL a C# 9 compiler could
//     have produced by hand — so the only thing gating any of it on a
//     given Unity version is whether its bundled compiler accepts the
//     right C# language version. Check docs/feature-matrix.md for the
//     up-to-date per-feature answer before shipping this shape of code
//     in a real project.
// JA: アイテム／インベントリシステム — loot table、ショップの在庫、
//     セーブデータの一覧 — は、どの Unity プロジェクトでも最もよく
//     見るゲームプレイコードの一つで、以下で現代化する、まさに C# 9
//     時代の習慣を溜め込みがちです。アイテムデータ用の mutable な
//     クラス、長い `if`/`else if` の階級判定、`.Add()` を積み重ねて
//     作る `new List<Item> { ... }`、そしてアイテムのフィールドから
//     計算する何かのための `static class ItemUtils` です。以下はすべて
//     コンパイル時の糖衣構文です — record、パターン、collection
//     expression、extension member はどれも、C# 9 のコンパイラが手作業
//     で生成できたであろう普通の IL にコンパイルされます — そのため、
//     ある Unity バージョンでこれらが使えるかどうかを決めるのは、
//     バンドルされたコンパイラが対応する C# 言語バージョンを受け付ける
//     かどうかだけです。実プロジェクトでこの形のコードを出荷する前に、
//     機能ごとの最新の答えを docs/feature-matrix.md で確認してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14

Item[] inventory = ???;

foreach (Item item in inventory)
{
    string tier = item switch
    {
        ??? => "common",
        { Value: < 100 } => "rare",
        _ => "legendary"
    };
    Console.WriteLine($"{item.Name}: {tier}, worth {item.TotalWorth}");
}

Console.WriteLine($"Grand total: {inventory.Sum(i => i.TotalWorth)}");

record Item(string Name, int Value, int Quantity);

static class ItemExtensions
{
    extension(Item item)
    {
        public int TotalWorth => ???;
    }
}

// HINT EN: Three blanks, one feature each. (1) `inventory` is a
//          collection expression holding three items: Potion (value
//          10, quantity 3), Elixir (value 50, quantity 1), Sword
//          (value 200, quantity 1) — `[new Item(...), new Item(...),
//          new Item(...)]`. (2) The first switch arm is a property
//          pattern matching directly on the record's `Value`, mirroring
//          the arm right below it: `{ Value: < 20 }`. (3) `TotalWorth`
//          multiplies the receiver's own properties: `item.Value *
//          item.Quantity`.
// HINT JA: 空欄は 3 つ、それぞれ別の機能です。(1) `inventory` は 3 つの
//          アイテムを持つ collection expression です。Potion（value
//          10、quantity 3）、Elixir（value 50、quantity 1）、Sword
//          （value 200、quantity 1）— `[new Item(...), new Item(...),
//          new Item(...)]` の形です。(2) 最初の switch アームは
//          record の `Value` に直接マッチするプロパティパターンで、
//          すぐ下のアームと同じ形にします。`{ Value: < 20 }` です。
//          (3) `TotalWorth` はレシーバー自身のプロパティ同士を掛けます。
//          `item.Value * item.Quantity` です。
//
// EXPECTED OUTPUT:
// Potion: common, worth 30
// Elixir: rare, worth 50
// Sword: legendary, worth 200
// Grand total: 280
