// [C# 10] readonly record struct
//
// EN: The mutable-by-default surprise: unlike `record class`, whose
//     positional properties are `{ get; init; }` (set only at
//     construction or via `with`), a plain `record struct`'s positional
//     properties are `{ get; set; }` — ordinary, mutable, assignable
//     any time. If you want the record-class-style guarantee that a
//     value never changes after construction, add `readonly`:
//     `readonly record struct` turns every positional property back
//     into a get-only member, so only `with` can produce a changed
//     value.
// JA: 「デフォルトで mutable」という驚きがあります。`record class` の
//     positional プロパティは `{ get; init; }`（構築時か `with` でしか
//     設定できません）なのに対し、ただの `record struct` の positional
//     プロパティは `{ get; set; }` — 普通の、いつでも代入できる mutable
//     なプロパティです。record class と同じように「構築後は値が変わら
//     ない」という保証がほしい場合は `readonly` を付けます。
//     `readonly record struct` にすると、すべての positional プロパティ
//     が get のみに戻り、値を変えられるのは `with` だけになります。
//
// Unity note:
// EN: You've already met this rule without the "record" part: writing
//     `transform.position.x = 5;` doesn't compile in Unity, because
//     `position` is a property returning a Vector3 by value — you
//     can't mutate a member of a value that isn't a variable. A
//     readonly record struct formalizes that same discipline for your
//     own value types, on purpose, instead of you discovering it by
//     accident on a property getter.
// JA: この規則には "record" を抜きにしてすでに出会っています。Unity で
//     `transform.position.x = 5;` はコンパイルが通りません。`position`
//     は Vector3 を値として返すプロパティであり、変数ではない値のメンバー
//     は変更できないからです。readonly record struct は、この同じ規律を
//     自分の値型に対して意図的に適用したものです。プロパティの getter で
//     偶然発見するのではなく、最初から明示しておく形です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#record-struct

var hp = new Health(100, 100);
hp.Current = 60;
Console.WriteLine(hp);
Console.WriteLine(hp.Current == hp.Max);

readonly record struct Health(int Current, int Max);

// HINT EN: `hp.Current = 60;` would compile for a plain, mutable
//          record struct, but `Health` is declared `readonly` here —
//          `Current` has no setter at all. Replace the direct
//          assignment with a non-destructive `with` expression that
//          reassigns `hp` to the new value it produces.
// HINT JA: `hp.Current = 60;` は普通の mutable な record struct なら
//          コンパイルが通りますが、ここでの `Health` は `readonly` で
//          宣言されており `Current` に setter はありません。直接代入を、
//          新しい値を生成する非破壊的な `with` 式に置き換え、その結果を
//          `hp` へ再代入してください。
//
// EXPECTED OUTPUT:
// Health { Current = 60, Max = 100 }
// False
