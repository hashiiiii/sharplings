// [C# 12] Inline arrays
//
// EN: A fixed-size buffer embedded directly inside a struct — no
//     separate heap array behind it — used to mean one of two
//     unpleasant choices. `unsafe fixed int _elements[8];` works, but
//     only for a handful of primitive types and only inside an
//     `unsafe` block. The C# 9-safe alternative was writing eight
//     separate named fields plus a hand-rolled indexer that switches
//     on the index — verbose, and easy to get wrong when the size
//     changes. The tempting "just use an array field" shortcut avoids
//     both problems syntactically, but puts a second heap object
//     behind every single instance: for a small fixed-size buffer
//     stored in thousands of entities, that is thousands of extra
//     allocations. C# 12 inline arrays close the gap: apply
//     `[InlineArray(N)]` (from `System.Runtime.CompilerServices`) to a
//     struct holding exactly one field of the element type, and the
//     compiler treats the whole struct as `N` contiguous slots of
//     that type — indexable with `[]`, walkable with `foreach`, and
//     convertible to a `Span<T>` — with no separate array allocation
//     at all.
// JA: struct の内側に直接埋め込まれた固定サイズのバッファ — 背後に
//     別のヒープ配列を持たないもの — は、これまで 2 つの好ましくない
//     選択肢を意味していました。`unsafe fixed int _elements[8];` は
//     動作しますが、一握りのプリミティブ型に限られ、`unsafe` ブロック
//     の中でしか使えません。C# 9 で safe な代替案は、8 個の個別の
//     名前付きフィールドと、インデックスで分岐する手書きのインデクサー
//     を書くことでした — 冗長で、サイズが変わると間違えやすいもの
//     です。「配列フィールドを使えばいい」という魅力的な近道は構文上
//     どちらの問題も避けられますが、インスタンス 1 つごとに 2 つ目の
//     ヒープオブジェクトを背後に持つことになります。小さな固定サイズ
//     バッファを何千ものエンティティに持たせるなら、それは何千もの
//     余分なアロケーションになります。C# 12 の inline array はこの
//     隙間を埋めます。要素型のフィールドをちょうど 1 つだけ持つ
//     struct に `[InlineArray(N)]`（`System.Runtime.CompilerServices`
//     由来）を付けると、コンパイラは struct 全体をその型の連続した
//     `N` 個のスロットとして扱います — `[]` でインデックスでき、
//     `foreach` で辿れて、`Span<T>` にも変換できます — しかも別の
//     配列アロケーションは一切ありません。
//
// Unity note:
// EN: Inline arrays are a pure compiler + runtime layout trick with no
//     equivalent on Unity's Mono/IL2CPP scripting backend through
//     6.7 — a struct decorated with `[InlineArray(N)]` simply is not
//     recognized as indexable/enumerable there the way it is here on
//     .NET 10, so code written this way fails outright on today's
//     Unity, not just "runs slower." The feature only becomes usable
//     once Unity ships its CoreCLR-based runtime in 6.8. See
//     docs/feature-matrix.md for exactly which Unity version / runtime
//     tier this needs.
// JA: inline array は純粋にコンパイラとランタイムのレイアウトの
//     仕組みであり、6.7 までの Unity の Mono / IL2CPP スクリプティング
//     バックエンドには相当するものがありません — `[InlineArray(N)]`
//     が付いた struct は、ここ（.NET 10）でのようにはインデックス可能
//     ／列挙可能として認識されず、このように書かれたコードは今日の
//     Unity では「遅くなる」どころか、そのまま動きません。この機能が
//     使えるようになるのは、Unity が CoreCLR ベースのランタイムを
//     6.8 で出荷してからです。どの Unity バージョン／ランタイム階層で
//     必要になるかは docs/feature-matrix.md を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#inline-arrays

Buffer8 rolls = default;
for (int i = 0; i < 8; i++)
    rolls[i] = i + 1;

int total = 0;
foreach (int roll in rolls)
    total += roll;

Console.WriteLine(total);

struct Buffer8
{
    private int _element0;
}

// HINT EN: `Buffer8` is meant to be an inline array over its one
//          `_element0` field, but nothing tells the compiler that —
//          right now it is just an ordinary one-field struct, which
//          cannot be indexed with `[]` or walked with `foreach`. Add
//          the missing `using` for the attribute's namespace, then
//          apply the attribute above the struct with the element
//          count this buffer needs.
// HINT JA: `Buffer8` は 1 つの `_element0` フィールドの上に成り立つ
//          inline array のつもりですが、それをコンパイラに伝えるものが
//          何もありません — 今のままではただの 1 フィールドの普通の
//          struct であり、`[]` でインデックスすることも `foreach` で
//          辿ることもできません。この属性の名前空間の `using` を
//          追加し、このバッファに必要な要素数とともに struct の上に
//          属性を付けてください。
//
// EXPECTED OUTPUT:
// 36
