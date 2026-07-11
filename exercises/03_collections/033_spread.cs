// [C# 12] The spread element
//
// EN: `..` inside a collection expression unpacks another collection's
//     elements in place, so `[..commonLoot, ..rareLoot, bonusCoin]`
//     reads left to right as "every element of `commonLoot`, then
//     every element of `rareLoot`, then the single value `bonusCoin`",
//     all copied into one new array. In C# 9 the same merge needed
//     `commonLoot.Concat(rareLoot).Append(bonusCoin).ToArray()` (LINQ)
//     or a hand-rolled loop with an `Array.Copy` / index-tracking
//     dance. `..` says the same thing without a method call in sight.
// JA: collection expression の中の `..` は、別のコレクションの要素を
//     その場に展開します。`[..commonLoot, ..rareLoot, bonusCoin]` は
//     左から右へ「`commonLoot` の全要素、続いて `rareLoot` の全要素、
//     続いて単一の値 `bonusCoin`」という意味になり、これらすべてが
//     1 つの新しい配列にコピーされます。C# 9 で同じ結合をするには
//     `commonLoot.Concat(rareLoot).Append(bonusCoin).ToArray()`
//     （LINQ）や、`Array.Copy` とインデックス管理を使った手書きの
//     ループが必要でした。`..` はメソッド呼び出しなしで同じことを
//     表現します。
//
// Unity note:
// EN: Combining a common drop table with a rare-tier bonus table is
//     exactly this shape: `[..commonLoot, ..rareLoot, bonusCoin]`
//     builds the final roll list in one line, in the order you read
//     it, with no `AddRange` calls and no intermediate `List<T>` you
//     have to remember to convert back to an array.
// JA: 共通ドロップ表とレアティアのボーナス表を結合する処理は、まさに
//     この形です。`[..commonLoot, ..rareLoot, bonusCoin]` は最終的な
//     抽選リストを 1 行で、読んだ通りの順序で構築し、`AddRange` の
//     呼び出しも、後で配列に戻すことを覚えておかなければならない
//     中間の `List<T>` も不要にします。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions#spread-element

int[] commonLoot = [10, 20, 30];
int[] rareLoot = [100, 200];
int bonusCoin = 999;

int[] dropTable = [..commonLoot, ???, bonusCoin];

Console.WriteLine(string.Join(' ', dropTable));
Console.WriteLine(dropTable.Length);

// HINT EN: `..commonLoot` is already spreading one array's elements
//          into the literal. `???` needs to do the same for the other
//          array declared above it — not just name it, but spread it
//          with `..`.
// HINT JA: `..commonLoot` はすでに 1 つの配列の要素をリテラルへ展開
//          しています。`???` には、上で宣言したもう一方の配列に対して
//          同じことをさせてください — 変数名を書くだけでなく `..` を
//          付けて展開する必要があります。
//
// EXPECTED OUTPUT:
// 10 20 30 100 200 999
// 6
