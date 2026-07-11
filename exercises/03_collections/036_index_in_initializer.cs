// [C# 13] Implicit index access in object initializers
//
// EN: The `^` (index-from-end) operator has worked in ordinary code
//     since C# 8 — `array[^1]` means "the last element" without
//     knowing the length. Until C# 13 it was not allowed inside a
//     nested object-initializer element assignment such as
//     `Members = { [^1] = ... }`. C# 13 lifts that restriction: within
//     an object initializer, `[^1]` now resolves against the length of
//     the collection being initialized, exactly like it would in a
//     regular statement.
// JA: `^`（末尾からのインデックス）演算子自体は C# 8 以降、通常の
//     コードでは `array[^1]`（長さを知らなくても「最後の要素」を
//     意味する）として使えていました。しかし C# 13 より前は、
//     `Members = { [^1] = ... }` のようなネストした object initializer
//     の要素代入の中では使用が許されていませんでした。C# 13 では
//     この制限が取り除かれ、object initializer の中でも `[^1]` は
//     通常の文と同じように、初期化対象コレクションの長さを基準に
//     解決されるようになりました。
//
// Unity note:
// EN: A fixed-size slot array — the last hotbar slot, the last item in
//     a preallocated buffer pool — is common in Unity code that avoids
//     resizing collections at runtime. Setting that trailing slot
//     inside the same object initializer that sets everything else
//     (`Members = { [0] = ..., [^1] = ... }`) reads as one declarative
//     block, instead of a constructor call followed by a separate
//     `.Members[^1] = ...;` statement underneath it.
// JA: 固定サイズのスロット配列 — ホットバーの最後のスロットや、あら
//     かじめ確保しておいたバッファプールの最後の要素 — は、実行時に
//     コレクションをリサイズしないようにする Unity のコードでよく
//     見られます。他のすべてを設定するのと同じ object initializer の
//     中で末尾のスロットも設定できると（`Members = { [0] = ...,
//     [^1] = ... }`）、コンストラクタ呼び出しの後に別で
//     `.Members[^1] = ...;` という文を書くのではなく、1 つの宣言的な
//     ブロックとして読めます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#implicit-index-access

var team = new Party
{
    Members =
    {
        [0] = "Tank",
        [1] = "Healer",
        [???] = "DPS"
    }
};

Console.WriteLine(string.Join(' ', team.Members));

class Party
{
    public string[] Members { get; init; } = new string[3];
}

// HINT EN: `Members` has 3 slots (indices 0, 1, 2). You could reach the
//          last one by writing the number 2 — but that hardcodes a
//          length you would have to update by hand if the array ever
//          grew. Use the index-from-end operator instead, so the
//          assignment stays correct no matter how many slots `Members`
//          has: `^1` always means "the last element."
// HINT JA: `Members` には 3 つのスロット（インデックス 0, 1, 2）が
//          あります。数値の 2 を書いても最後の要素に届きますが、それ
//          では配列が大きくなるたびに手で書き換えなければならない
//          長さをハードコードすることになります。代わりに末尾からの
//          インデックス演算子を使えば、`Members` のスロット数が
//          いくつであっても代入が正しいままになります — `^1` は常に
//          「最後の要素」を意味します。
//
// EXPECTED OUTPUT:
// Tank Healer DPS
