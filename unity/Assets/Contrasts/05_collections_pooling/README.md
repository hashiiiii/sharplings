# 05_collections_pooling

**EN:** `Before/CollectionsPoolingBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/CollectionsPoolingAfter.cs` mixes features with different requirements -- see "Which Unity version/runtime this needs" below for the precise breakdown. Its folder name ends in `~`, which makes it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix.

**JA:** `Before/CollectionsPoolingBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/CollectionsPoolingAfter.cs` は、必要条件の異なる機能が混ざっています -- 正確な内訳は下の「どの Unity バージョン/ランタイムが必要か」を参照してください。フォルダ名の末尾が `~` なので Unity の importer から見えず、今の project を壊すことはありません。機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The C# 9 Unity idiom for combining and filtering small arrays is manual array juggling plus a fresh `List<T>` per call:

```csharp
var combined = new int[baseHits.Length + extraHits.Length];
Array.Copy(baseHits, combined, baseHits.Length);
Array.Copy(extraHits, 0, combined, baseHits.Length, extraHits.Length);

var result = new List<int>();
for (int i = 0; i < source.Length; i++)
{
    if (source[i] > 30) result.Add(source[i]);
}
```

This works and is easy to read once you know the pattern, but every call allocates: the combined array, and a brand-new `List<T>` (plus its internal backing array) for the filtered result. Fine for a one-off; wasteful if this runs every frame, which is exactly the shape a lot of per-frame Unity gameplay code takes.

**JA:** 小さな配列を結合してフィルタする C# 9 の Unity イディオムは、手動の配列操作と、呼び出しごとに新しく作る `List<T>` です。

```csharp
var combined = new int[baseHits.Length + extraHits.Length];
Array.Copy(baseHits, combined, baseHits.Length);
Array.Copy(extraHits, 0, combined, baseHits.Length, extraHits.Length);

var result = new List<int>();
for (int i = 0; i < source.Length; i++)
{
    if (source[i] > 30) result.Add(source[i]);
}
```

これは動きますし、パターンさえ分かれば読みやすくもあります。しかし呼び出すたびに割り当てが発生します -- 結合用の配列、そしてフィルタ結果のための真新しい `List<T>`（とその内部の backing array）です。一度きりなら問題ありませんが、毎フレーム実行されるなら無駄が積み重なります。そして、これはまさに多くの Unity の毎フレームのゲームプレイコードが取る形です。

## Why the After form is better

**EN:** `After~/CollectionsPoolingAfter.cs` replaces the manual copy with a collection expression and spread:

```csharp
int[] combined = [.. baseHits, .. extraHits];
```

and replaces the fresh `List<T>` with a scratch buffer **rented from `ArrayPool<int>.Shared`**, sliced with `Span<T>` down to just the matches, and returned to the pool when done:

```csharp
int[] buffer = ArrayPool<int>.Shared.Rent(source.Length);
try
{
    Span<int> scratch = buffer;
    int count = 0;
    foreach (int value in source)
    {
        if (value > 30) scratch[count++] = value;
    }
    return scratch[..count].ToArray();
}
finally
{
    ArrayPool<int>.Shared.Return(buffer);
}
```

The collection expression reads as data, not as a sequence of copy instructions. The pooled buffer means repeated calls reuse the same backing memory instead of allocating a new one (and a new `List<T>` wrapper) every time -- the allocation profile a hot per-frame path actually wants.

**JA:** `After~/CollectionsPoolingAfter.cs` は手動コピーを collection expression と spread に置き換えます。

```csharp
int[] combined = [.. baseHits, .. extraHits];
```

そして真新しい `List<T>` を、**`ArrayPool<int>.Shared` から rent したスクラッチバッファ** に置き換えます。`Span<T>` でマッチした分だけに slice し、使い終わったら pool に返します。

```csharp
int[] buffer = ArrayPool<int>.Shared.Rent(source.Length);
try
{
    Span<int> scratch = buffer;
    int count = 0;
    foreach (int value in source)
    {
        if (value > 30) scratch[count++] = value;
    }
    return scratch[..count].ToArray();
}
finally
{
    ArrayPool<int>.Shared.Return(buffer);
}
```

collection expression は、コピー手順の並びではなく、データそのものとして読めます。pooled バッファのおかげで、繰り返し呼び出しても同じ backing memory を使い回せます -- 毎回新しく確保する（しかも新しい `List<T>` の wrapper 込みで）代わりに。これは、毎フレーム実行されるホットパスが本当に求めている割り当てのプロファイルです。

## Which Unity version/runtime this needs

**EN:**

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| Collection expressions (`[10, 20, 30]`) and spread (`[.. baseHits, .. extraHits]`) | C# 12 | `yes — confirmed 2026-07-12 (Stage 1, editor batchmode)` per `docs/feature-matrix.md`'s Stage 1 results | A Stage 1 `csc.rsp` bump today, unofficially; official on Unity 6.8 |
| `Span<T>` / `ReadOnlySpan<T>` slicing (`scratch[..count]`) | C# 7.2 (ref structs), netstandard 2.1 BCL type | Yes | Nothing extra -- already available under Unity's default C# 9 |
| `System.Buffers.ArrayPool<T>` | Plain BCL API (no language-version gate) | Yes | Nothing extra -- ships in netstandard 2.1, already available today |

Only the collection-expression/spread syntax is actually gated, and only as far as Stage 1 (`docs/unity-lab-setup.md`) -- not Stage 2 or Unity 6.8. `Span<T>` and `ArrayPool<T>` already work in `Assets/Contrasts` as it stands. This file still lives in `After~` as a whole, rather than splitting the collection-expression lines out into `Before`, to keep the six topics' shape consistent and to keep `Assets/Contrasts` itself free of any langversion dependency by design.

**JA:**

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| collection expression（`[10, 20, 30]`）と spread（`[.. baseHits, .. extraHits]`） | C# 12 | `docs/feature-matrix.md` の Stage 1 の結果によれば `yes — confirmed 2026-07-12 (Stage 1, editor batchmode)` | 今日は非公式に Stage 1 の `csc.rsp` 引き上げ。Unity 6.8 では公式 |
| `Span<T>` / `ReadOnlySpan<T>` の slicing（`scratch[..count]`） | C# 7.2（ref struct）、netstandard 2.1 の BCL 型 | 可能 | 追加なし -- Unity の既定 C# 9 の下ですでに使えます |
| `System.Buffers.ArrayPool<T>` | 素の BCL API（言語バージョンの制約なし） | 可能 | 追加なし -- netstandard 2.1 に同梱済みで、今日すでに使えます |

実際に制約がかかっているのは collection expression / spread の構文だけで、しかも Stage 1（`docs/unity-lab-setup.md`）止まりです -- Stage 2 も Unity 6.8 も必要ありません。`Span<T>` と `ArrayPool<T>` は、今の `Assets/Contrasts` のままですでに使えます。それでもこのファイル全体を `After~` に置いているのは、collection expression の行だけを `Before` に切り出さず、6 topic の形を揃えるためと、`Assets/Contrasts` 自体を設計上どの langversion 依存からも自由に保つためです。

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix (see "Collection expressions (C# 12)").
- `../../../../docs/unity-lab-setup.md` -- Stage 1 setup (section 3), if you want to try the collection-expression syntax today.
