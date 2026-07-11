// [C# 14] The field keyword
//
// EN: A validated auto-property has always meant giving up on "auto":
//     you declared a private backing field, wrote a getter returning
//     it and a setter clamping into it, and picked a name for that
//     field that had nothing to do with the public property's own
//     name. C# 14's contextual `field` keyword removes the field
//     declaration entirely — inside a property's accessors, `field`
//     refers to the compiler-generated backing storage directly, the
//     same storage a plain `{ get; set; }` auto-property already has.
//     You get validation or a computed default without hand-declaring
//     anything: just a `get` and `set` written in terms of `field`.
// JA: これまで、値を検証する auto-property を書くには「auto」であるこ
//     とをあきらめる必要がありました。private なバッキングフィールドを
//     宣言し、それを返す getter と、それへ値をクランプして書き込む
//     setter を書き、さらに公開プロパティ自身の名前とは無関係な名前を
//     そのフィールドに付けていました。C# 14 のコンテキストキーワード
//     `field` は、このフィールド宣言そのものを不要にします。プロパティ
//     のアクセサの中で `field` は、普通の `{ get; set; }` auto-property
//     がすでに持っているのと同じ、コンパイラが生成するバッキングスト
//     レージを直接指します。何も自分で宣言することなく、`field` を使っ
//     た `get` と `set` を書くだけで検証や計算済みの初期値を得られます。
//
// Unity note:
// EN: `[SerializeField] private int level;` plus a public wrapper
//     property that clamps on the way in is a pattern in almost every
//     Unity codebase — stats, health, anything the Inspector edits
//     that must never go negative. `field` collapses that pattern:
//     one property, no separately named backing field to keep in sync
//     with the Inspector-visible one.
// JA: `[SerializeField] private int level;` と、それを書き込み時に
//     クランプする公開ラッパープロパティの組み合わせは、ほぼすべての
//     Unity コードベースに現れるパターンです — ステータスや体力など、
//     Inspector から編集できて負の値になってはいけない値はすべてこれ
//     に当たります。`field` はこのパターンを 1 つのプロパティに集約し
//     ます。Inspector に見えているフィールドと同期を保つための、別名
//     のバッキングフィールドはもう不要です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#the-field-keyword

var character = new Character();
Console.WriteLine(character.Level);

character.Level = 5;
Console.WriteLine(character.Level);

character.Level = -3;
Console.WriteLine(character.Level);

class Character
{
    public int Level
    {
        get => field;
        set => field = ???;
    } = 1;
}

// HINT EN: ??? should clamp `value` so `Level` never drops below 1 —
//          compare it against 1 and fall back to 1 when it's lower,
//          otherwise keep `value` as given.
// HINT JA: ??? は `Level` が 1 を下回らないよう `value` をクランプする
//          式です。`value` を 1 と比較し、1 より小さければ 1 を、そう
//          でなければ `value` をそのまま使ってください。
//
// EXPECTED OUTPUT:
// 1
// 5
// 1
