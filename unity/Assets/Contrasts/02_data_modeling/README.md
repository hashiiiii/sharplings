# 02_data_modeling

**EN:** `Before/DataModelingBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/DataModelingAfter.cs` does **not** compile today, and for a more specific reason than most of these six topics: see "Which Unity version/runtime this needs" below. Its folder name ends in `~`, which makes it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix.

**JA:** `Before/DataModelingBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/DataModelingAfter.cs` は今日は **コンパイルできません** -- しかも、この 6 topic の中でもやや特有の理由からです。詳細は下の「どの Unity バージョン/ランタイムが必要か」を参照してください。フォルダ名の末尾が `~` なので Unity の importer から見えず、今の project を壊すことはありません。機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The C# 9 Unity idiom for a small data-transfer shape is a mutable `[Serializable]` class with a hand-written constructor:

```csharp
[Serializable]
public class ItemStackBefore
{
    public string Id;
    public int Count;

    public ItemStackBefore(string id, int count)
    {
        Id = id;
        Count = count;
    }
}
```

`Before/DataModelingBefore.cs` demonstrates the two costs of this shape: "picking up one more" means mutating the instance in place (`potions.Count += 1;`), and two instances built from identical data are not `==`/`.Equals()`-equal unless you hand-write that comparison too -- `ItemStackBefore` inherits plain reference equality from `object`.

**JA:** ちょっとしたデータ転送用の形として、C# 9 の Unity イディオムは、手書きの constructor を持つ mutable な `[Serializable]` class です。

```csharp
[Serializable]
public class ItemStackBefore
{
    public string Id;
    public int Count;

    public ItemStackBefore(string id, int count)
    {
        Id = id;
        Count = count;
    }
}
```

`Before/DataModelingBefore.cs` は、この形が抱える 2 つのコストを示しています。「もう 1 つ拾う」ことは、インスタンスをその場で mutate することを意味します（`potions.Count += 1;`）。そして、同じデータから作った 2 つのインスタンスは、比較を自分で書かない限り `==` / `.Equals()` で等しくなりません -- `ItemStackBefore` は `object` からただの参照等価性を継承しているだけです。

## Why the After form is better

**EN:** `After~/DataModelingAfter.cs` rewrites the same shape as a `record`:

```csharp
public record ItemStackAfter(string Id, int Count);
```

This one line gives you a constructor, `Deconstruct`, `ToString`, and -- the two things `Before` had to do without -- **value equality** (`potions == potionsAgain` is `true` when the data matches, no override needed) and **immutability by default** (there is no setter to mutate; "picking up one more" is `potions = potions with { Count = potions.Count + 1 };`, which produces a new value instead of changing the old one). `ItemSlotAfter` goes one step further with a `record struct` plus `required`/`init` members, for a small, frequently-copied value type where every field must be set at construction and never changed afterward.

**JA:** `After~/DataModelingAfter.cs` は同じ形を `record` として書き直します。

```csharp
public record ItemStackAfter(string Id, int Count);
```

この 1 行だけで、constructor、`Deconstruct`、`ToString`、そして -- `Before` には無かった 2 つのもの -- **値の等価性**（データが一致すれば `potions == potionsAgain` は override 無しで `true` になります）と **既定での不変性**（mutate する setter が存在しません。「もう 1 つ拾う」は `potions = potions with { Count = potions.Count + 1 };` となり、古い値を変えるのではなく新しい値を作ります）が手に入ります。`ItemSlotAfter` はさらに一歩進んで、`record struct` と `required` / `init` member を組み合わせ、すべての field を construction 時に必ず設定し、以後は変更させない、小さく頻繁にコピーされる値型にしています。

## Honest note: Unity's serializer does not handle records

**EN:** Do not swap `[Serializable]` inspector-exposed fields for records. Unity's serializer works by reflecting over public fields (or `[SerializeField]` private fields) and needs a parameterless constructor it can call itself -- a `record`'s compiler-generated positional constructor and init-only properties do not fit that shape, and Unity's Inspector will not show or persist them correctly. Keep the mutable `[Serializable]` class from `Before/DataModelingBefore.cs` for anything a designer edits in the Inspector or that gets serialized into a scene/prefab/`ScriptableObject`. Reach for `record`/`record struct` only for **runtime domain data**: values that flow between systems in code, event payloads, intermediate results -- never data the Inspector needs to draw or a scene file needs to store.

**JA:** `[Serializable]` で Inspector に出す field を record に置き換えないでください。Unity のシリアライザは public field（または `[SerializeField]` の private field）を reflection で走査し、自分で呼び出せる引数無し constructor を必要とします -- `record` がコンパイラ生成する位置引数 constructor と init-only なプロパティは、この形に合いません。Unity の Inspector はそれらを正しく表示・保存できません。デザイナーが Inspector で編集するもの、あるいは scene / prefab / `ScriptableObject` へシリアライズされるものには、`Before/DataModelingBefore.cs` の mutable な `[Serializable]` class を使い続けてください。`record` / `record struct` は **runtime のドメインデータ** -- コード内でシステム間を流れる値、イベントの payload、中間結果など -- にだけ使ってください。Inspector が描画する必要があるデータや、scene ファイルが保存する必要があるデータには使わないでください。

## Which Unity version/runtime this needs

**EN:** This topic needs more care than "C# 14 vs today" -- verified locally against this exact editor install (2026-07-12): a plain `record`/`init` type does **not** compile in Unity 6000.7.0a2 as shipped, even though `record`/`init` are C# 9 features and C# 9 is Unity's own default `LangVersion`. Batchmode reports:

```
error CS0518: Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
```

Unity's bundled Mono corelib does not expose the `IsExternalInit` marker type that `init` accessors compile against (a plain .NET SDK build gets this for free because `dotnet build`'s own MSBuild targets inject a small polyfill source file automatically for down-level target frameworks; Unity's build pipeline does not go through those targets, so nothing supplies it). This is exactly the same category of gap `docs/feature-matrix.md` already documents for **required members** (`RequiredMemberAttribute` / `SetsRequiredMembersAttribute` / `CompilerFeatureRequiredAttribute`) -- `ItemSlotAfter`'s `required` members hit that gap too, on top of `IsExternalInit`.

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| `record` / `init` (`ItemStackAfter`, and the property syntax on `ItemSlotAfter`) | C# 9 | Langversion is not the blocker -- Unity's Mono corelib lacks `IsExternalInit` (verified locally, 2026-07-12) | [PolySharp](https://github.com/Sergio0694/PolySharp)'s shim (`docs/unity-lab-setup.md` section 6) today, unofficially; nothing extra on **Unity 6.8**'s real .NET 10 BCL |
| `required` members (`ItemSlotAfter`) | C# 11 | `expected — accepts ≤ 12 (Stage 1)` per Stage 0, but Mono needs `Needs PolySharp` (`docs/feature-matrix.md`) | Same PolySharp shim as above, today; nothing extra on Unity 6.8 |

Practically: this whole file needs, at minimum, PolySharp installed in the Lab (`docs/unity-lab-setup.md` section 6) plus a Stage 1/2 `csc.rsp` langversion bump to compile anywhere in this Unity install today -- or simply **Unity 6.8**, whose real .NET 10 BCL ships all three marker types natively (`docs/feature-matrix.md`'s "PolySharp no longer needed" note). Either way, that is outside `Assets/Contrasts`'s scope (kept dependency-free and Stage-0/isolated by design), which is exactly why this lives in `After~` rather than `After`.

**JA:** この topic は「今日 vs C# 14」よりも注意が必要です -- この正確な editor install に対して 2026-07-12 にローカルで確認したところ、`record` / `init` は C# 9 の機能であり、C# 9 は Unity 自身の既定 `LangVersion` であるにもかかわらず、Unity 6000.7.0a2 の出荷状態では素の `record` / `init` 型は **コンパイルできません**。batchmode は次を報告します。

```
error CS0518: Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
```

Unity 同梱の Mono corelib には、`init` アクセサがコンパイル時に参照する `IsExternalInit` マーカー型がありません（素の .NET SDK build がこれを無償で得られるのは、`dotnet build` 自身の MSBuild targets が、より低い target framework 向けに小さな polyfill source file を自動的に注入するためです。Unity の build pipeline はその targets を経由しないため、何もそれを供給しません）。これは、`docs/feature-matrix.md` がすでに文書化している **required members**（`RequiredMemberAttribute` / `SetsRequiredMembersAttribute` / `CompilerFeatureRequiredAttribute`）とまったく同じ種類の gap です -- `ItemSlotAfter` の `required` member は、`IsExternalInit` に加えてこの gap にも当たります。

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| `record` / `init`（`ItemStackAfter`、および `ItemSlotAfter` のプロパティ構文） | C# 9 | langversion は障壁ではありません -- Unity の Mono corelib に `IsExternalInit` が無いのが原因です（2026-07-12 にローカルで確認済み） | 今日は非公式に [PolySharp](https://github.com/Sergio0694/PolySharp) の shim（`docs/unity-lab-setup.md` 6 節）。**Unity 6.8** の実際の .NET 10 BCL では追加不要 |
| `required` member（`ItemSlotAfter`） | C# 11 | Stage 0 によれば `expected — accepts ≤ 12 (Stage 1)` だが、Mono では `Needs PolySharp`（`docs/feature-matrix.md`） | 今日は上記と同じ PolySharp shim。Unity 6.8 では追加不要 |

実務的には、このファイル全体を今のこの Unity install の中でどこかでコンパイルするには、最低でも Lab に PolySharp を install（`docs/unity-lab-setup.md` 6 節）した上で Stage 1/2 の `csc.rsp` langversion 引き上げが必要です -- あるいは単純に **Unity 6.8** であれば、その実際の .NET 10 BCL が 3 つのマーカー型すべてをネイティブに持っています（`docs/feature-matrix.md` の「PolySharp no longer needed」の note）。いずれにせよ、これは `Assets/Contrasts` の scope の外です（依存関係を持たず Stage 0 のまま隔離しておく設計のため）-- だからこそこれは `After` ではなく `After~` に置かれています。

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix (see the "Required members" and "Needs PolySharp polyfill" rows).
- `../../../../docs/unity-lab-setup.md` -- Stage 0-2 procedures and PolySharp installation (section 6).
