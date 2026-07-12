# 01_null_handling

**EN:** `Before/NullHandlingBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/NullHandlingAfter.cs` targets C# 14 and needs either **Unity 6.8** (official) or a **Stage 2** Roslyn swap (unofficial, see `docs/unity-lab-setup.md`) to compile; its folder name ends in `~`, which makes it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix this note is drawn from.

**JA:** `Before/NullHandlingBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/NullHandlingAfter.cs` は C# 14 を対象としており、コンパイルには **Unity 6.8**（公式）または **Stage 2** の Roslyn 差し替え（非公式、`docs/unity-lab-setup.md` 参照）のどちらかが必要です。フォルダ名の末尾が `~` なので Unity の importer から見えず、今の project を壊すことはありません。この note のもとになった機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The C# 9 Unity idiom for checking whether a reference to a `UnityEngine.Object` (a `MonoBehaviour`, `GameObject`, `Component`, ...) is safe to use is an explicit chain:

```csharp
if (target != null && target.enabled)
{
    // ...
}
```

This looks like ordinary null-checking, but it is doing something Unity-specific: `UnityEngine.Object` overloads `==` and `!=` so that a **destroyed** object still compares equal to `null`, even though the underlying C# reference is not actually null. This is Unity's well-known "fake null". A `MonoBehaviour` whose `GameObject` has been `Destroy()`-ed keeps its managed wrapper around (subject to GC) with its native side gone; `target != null` calls into that overload and correctly reports "gone", which is exactly what `Before/NullHandlingBefore.cs`'s `ReportTarget` method demonstrates: the same check reports "alive and enabled" before `DestroyImmediate` and "null-or-disabled" right after, with no change to the check itself.

**JA:** `UnityEngine.Object`（`MonoBehaviour`、`GameObject`、`Component` など）への参照が安全に使えるかを確認する C# 9 の Unity イディオムは、明示的な連鎖です。

```csharp
if (target != null && target.enabled)
{
    // ...
}
```

これは一見ふつうの null チェックですが、実際には Unity 固有の処理をしています。`UnityEngine.Object` は `==` と `!=` をオーバーロードしており、**破棄済み** のオブジェクトは、内部の C# 参照が実際には null でなくても null と等しく比較されます。これが Unity でよく知られた「fake null」です。`GameObject` が `Destroy()` された `MonoBehaviour` は、managed 側の wrapper は（GC 対象として）残ったまま、native 側だけが失われます。`target != null` はこのオーバーロードを経由するため「無くなった」ことを正しく報告します -- これはまさに `Before/NullHandlingBefore.cs` の `ReportTarget` メソッドが示す通りです。`DestroyImmediate` の前は「alive and enabled」、直後は「null-or-disabled」と、チェック自体を変えずに報告が変わります。

## Why the After form is better -- and its one trap

**EN:** `After~/NullHandlingAfter.cs` keeps that exact `if (target != null && target.enabled)` check **unchanged**. That is deliberate, not an oversight: it is the one place in this whole exercise set where writing more "modern" C# would *silently change behavior*.

- Pattern matching's null test (`is null`, `is not null`, and the implicit null check inside a property pattern like `is { enabled: true }`) is a **raw reference check**, by design -- it exists specifically so user code cannot make `is null` lie by overloading `==`. But that same design choice means it bypasses `UnityEngine.Object`'s overload entirely.
- The null-conditional operator `?.` (and now, in C# 14, null-conditional *assignment*, `?.` on the left of `=`) has the same raw-reference null test under the hood.

So `if (target is { enabled: true })` or `target?.enabled == true` look like reasonable modernizations of the Before idiom, but a destroyed-but-fake-null `target` sails straight through both of them -- they report "alive" (or throw down inside `.enabled`) exactly when the correct answer is "destroyed". `After~/NullHandlingAfter.cs`'s `ReportTarget` method carries a `PITFALL` comment spelling this out at the exact line it would bite. The rule of thumb: **for any `UnityEngine.Object` reference, keep the explicit `!= null`/`== null` check, in any C# version.** Reserve pattern matching's null tests and `?.`/null-conditional assignment for plain C# reference types, where they mean exactly what they look like they mean.

That plain-C#-type case is where C# 14 is a real, unambiguous win, and `ReportStats`/the `Stats` class in both files demonstrate it: Before needs `if (_liveStats != null) { _liveStats.Score = 99; }`; After collapses that to `_liveStats?.Score = 99;` -- a no-op if the receiver is null (right-hand side not even evaluated), the same assignment otherwise. `Stats` is a plain class, not a `UnityEngine.Object`, so there is no fake-null to worry about and the collapse is completely safe.

**JA:** `After~/NullHandlingAfter.cs` は `if (target != null && target.enabled)` というチェックを **そのまま変えていません**。これは見落としではなく意図的です -- この演習セット全体の中で、より「モダン」な C# を書くことが *挙動を静かに変えてしまう* ただ 1 か所だからです。

- パターンマッチングの null テスト（`is null`、`is not null`、そして `is { enabled: true }` のような property パターン内部の暗黙の null チェック）は、設計上 **生の参照チェック** です -- これは、ユーザーコードが `==` をオーバーロードして `is null` に嘘をつかせられないようにするための、意図的な設計です。しかし同じ設計判断のせいで、`UnityEngine.Object` のオーバーロードを完全に迂回してしまいます。
- null 条件演算子 `?.`（そして C# 14 で新しく加わった null 条件付き **代入**、`=` の左側の `?.`）も、内部では同じ生の参照チェックを行います。

そのため `if (target is { enabled: true })` や `target?.enabled == true` は、Before のイディオムの妥当なモダン化に見えますが、破棄済みで fake null な `target` はどちらもすり抜けてしまいます -- 正しい答えが「破棄済み」であるまさにそのときに、「alive」と報告する（あるいは `.enabled` の奥で例外を投げる）のです。`After~/NullHandlingAfter.cs` の `ReportTarget` メソッドには、まさにその行に `PITFALL` コメントでこれを明記しています。経験則はこうです -- **`UnityEngine.Object` への参照には、どの C# バージョンであっても明示的な `!= null` / `== null` チェックを使い続けること。** パターンマッチングの null テストや `?.` / null 条件付き代入は、見た目どおりの意味を持つ素の C# 参照型のために取っておいてください。

その素の C# 型のケースこそ、C# 14 が本当に明確な勝ちになる場所です。両ファイルの `ReportStats` / `Stats` クラスがそれを示しています -- Before は `if (_liveStats != null) { _liveStats.Score = 99; }` が必要ですが、After はそれを `_liveStats?.Score = 99;` へ畳みます -- 受け手が null なら何もしません（右辺は評価すらされません）、そうでなければ同じ代入が起こります。`Stats` は `UnityEngine.Object` ではなくただのクラスなので fake null の心配がなく、この畳み込みは完全に安全です。

## Which Unity version/runtime this needs

**EN:**

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| `!= null` / `== null` chain (Before, and the Unity-object check in After) | C# 1 | Yes, always | Nothing extra -- Unity's default C# 9 |
| Property patterns (`is { enabled: true }`), referenced only in the PITFALL comment | C# 8 | Yes | Nothing extra -- already legal under Unity's default C# 9, per `docs/feature-matrix.md`'s Stage 0 findings (bundled Roslyn 4.10 defaults to 12, accepts C# 8 trivially) |
| Null-conditional assignment (`_liveStats?.Score = 99;`) | **C# 14** | No -- `docs/feature-matrix.md`'s Stage 0 probe found Unity's bundled Roslyn 4.10 tops out at `-langversion 12` (plus `preview`/`latest`/`latestmajor`); C# 14 needs a Stage 2 swap | **Unity 6.8** (official), or Stage 2 (`docs/unity-lab-setup.md`) unofficially, today |

**JA:**

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| `!= null` / `== null` の連鎖（Before、および After の Unity object チェック） | C# 1 | 常に可能 | 追加なし -- Unity の既定 C# 9 のまま |
| property パターン（`is { enabled: true }`、PITFALL コメント内でのみ言及） | C# 8 | 可能 | 追加なし -- `docs/feature-matrix.md` の Stage 0 の結果どおり、Unity の既定 C# 9 の下ですでに合法（同梱 Roslyn 4.10 は既定 12 で、C# 8 は問題なく受け付ける） |
| null 条件付き代入（`_liveStats?.Score = 99;`） | **C# 14** | 不可 -- `docs/feature-matrix.md` の Stage 0 probe によれば、Unity 同梱の Roslyn 4.10 は `-langversion 12`（および `preview`/`latest`/`latestmajor`）が上限。C# 14 には Stage 2 の差し替えが必要 | 今日の時点では **Unity 6.8**（公式）、または非公式に Stage 2（`docs/unity-lab-setup.md`） |

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix.
- `../../../../docs/unity-lab-setup.md` -- Stage 0-2 procedures (the unofficial ways to unlock newer C# inside this same Unity install, ahead of 6.8).
- `../../../../exercises/08_extensions/083_null_conditional_assignment.cs` -- the plain-.NET exercise this Unity note pairs with.
