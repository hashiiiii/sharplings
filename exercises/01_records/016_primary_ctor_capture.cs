// [C# 12] Primary constructor capture semantics
//
// EN: A primary constructor parameter referenced from an instance
//     member (not just the constructor itself) is captured into one
//     private field, shared by every member that reads it — mutate it
//     in one method, and every other member sees the new value, the
//     same way a closure shares one variable instead of taking an
//     independent copy per use. The classic double-capture trap:
//     if you ALSO assign the parameter into a property initializer
//     (`= hits`), that grabs its own one-time snapshot at
//     construction — a second, separate storage location — so the
//     shared field and the frozen snapshot silently drift apart the
//     moment the shared field is mutated. (The compiler even warns
//     about this shape: CS9124.)
// JA: プライマリコンストラクタのパラメータが（コンストラクタ自身だけで
//     なく）インスタンスメンバーから参照されると、1 つの private field
//     に capture され、それを読むすべてのメンバーで共有されます。
//     あるメソッドで変更すると、他のすべてのメンバーからも新しい値が
//     見えます。クロージャが変数を共有し、使用ごとに独立したコピーを
//     取らないのと同じです。典型的な「二重 capture」の罠はここに
//     あります。同じパラメータをプロパティの初期化子（`= hits`）にも
//     代入すると、そちらは構築時点の一度きりのスナップショットを別の
//     独立した保存先に取得してしまい、共有 field が変更された瞬間に、
//     共有 field と固定されたスナップショットが静かにずれ始めます
//     （コンパイラはこの形に対して実際に CS9124 という警告を出します）。
//
// Unity note:
// EN: Picture a plain C# `ComboCounter` that a MonoBehaviour holds
//     onto and calls into every time the player lands a hit — exactly
//     the kind of service class 015 introduced. If one member updates
//     the running count and another member is supposed to report the
//     current total, this is the bug that silently ships: the UI
//     keeps showing the number the counter started with.
// JA: プレイヤーがヒットを当てるたびに MonoBehaviour が保持して呼び出す、
//     ただの C# の `ComboCounter` を思い浮かべてください。015 で紹介した
//     ようなサービスクラスそのものです。あるメンバーが進行中のカウントを
//     更新し、別のメンバーが現在の合計を報告するはずになっていると、
//     気づかないままリリースされるバグはこうなります。UI にはカウンター
//     が開始した時点の数字が表示され続けます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors

var combo = new ComboCounter(0);
combo.RegisterHit();
combo.RegisterHit();
Console.WriteLine(combo.RegisterHit());
Console.WriteLine(combo.Total);

class ComboCounter(int hits)
{
    public int RegisterHit()
    {
        hits++;
        return hits;
    }

    public int Total { get; } = hits;
}

// HINT EN: `RegisterHit` mutates the captured `hits` field directly.
//          `Total` instead grabs its OWN snapshot of `hits` once, at
//          construction time (when it was still 0), and never
//          updates. Make `Total` read the same captured field
//          `RegisterHit` shares by turning it into an expression-
//          bodied property instead of an initialized auto-property.
// HINT JA: `RegisterHit` は capture された `hits` field を直接変更
//          します。一方 `Total` は構築時点（まだ 0 だった時点）で
//          `hits` 自身のスナップショットを一度だけ取得し、以降更新
//          されません。初期化子付きの自動プロパティをやめ、式形式の
//          プロパティにして、`RegisterHit` と同じ capture された
//          field を読むようにしてください。
//
// EXPECTED OUTPUT:
// 3
// 3
