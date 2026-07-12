# 06_static_utilities

**EN:** `Before/StaticUtilitiesBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/StaticUtilitiesAfter.cs` targets C# 14 and needs either **Unity 6.8** (official) or a **Stage 2** Roslyn swap (unofficial, see `docs/unity-lab-setup.md`) to compile; its folder name ends in `~`, which makes it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix.

**JA:** `Before/StaticUtilitiesBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/StaticUtilitiesAfter.cs` は C# 14 を対象としており、コンパイルには **Unity 6.8**（公式）または **Stage 2** の Roslyn 差し替え（非公式、`docs/unity-lab-setup.md` 参照）のどちらかが必要です。フォルダ名の末尾が `~` なので Unity の importer から見えず、今の project を壊すことはありません。機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The C# 9 Unity idiom for adding behavior to a type you don't own (like `Vector3`) is a static helper class, with the subject passed as an ordinary first argument:

```csharp
public static class VectorUtilsBefore
{
    public static Vector3 WithY(Vector3 v, float y) => new Vector3(v.x, y, v.z);
    public static float HorizontalSqrMagnitude(Vector3 v) => v.x * v.x + v.z * v.z;
}

// call site:
var grounded = VectorUtilsBefore.WithY(position, 0f);
var horizontalSqrMag = VectorUtilsBefore.HorizontalSqrMagnitude(position);
```

This works, but every call site reads "outside-in": the class name comes first, then the subject, buried as an argument, rather than reading left-to-right the way `position.WithY(0f)` would. There is also no way to express something that should read like a *property* (`HorizontalSqrMagnitude` is conceptually a property of a `Vector3`, not an action) -- a static method call is the only shape available.

**JA:** 自分が所有していない型（`Vector3` のような）に振る舞いを足す C# 9 の Unity イディオムは、対象を普通の第一引数として渡す static ヘルパー class です。

```csharp
public static class VectorUtilsBefore
{
    public static Vector3 WithY(Vector3 v, float y) => new Vector3(v.x, y, v.z);
    public static float HorizontalSqrMagnitude(Vector3 v) => v.x * v.x + v.z * v.z;
}

// 呼び出し側:
var grounded = VectorUtilsBefore.WithY(position, 0f);
var horizontalSqrMag = VectorUtilsBefore.HorizontalSqrMagnitude(position);
```

これは動きますが、呼び出し側は「外から内へ」読むことになります -- class 名が先に来て、対象は引数の中に埋もれており、`position.WithY(0f)` のように左から右へ読める形にはなりません。また、*プロパティ* のように読めるべきもの（`HorizontalSqrMagnitude` は概念上 `Vector3` のプロパティであり、動作ではありません）を表現する方法もありません -- static メソッド呼び出しという形しか選べないのです。

## Why the After form is better

**EN:** `After~/StaticUtilitiesAfter.cs` uses C# 14 **extension members** -- an `extension(Vector3 v)` block inside the same static class:

```csharp
extension(Vector3 v)
{
    public float HorizontalSqrMagnitude => v.x * v.x + v.z * v.z;
    public Vector3 WithY(float y) => new(v.x, y, v.z);
}
```

Call sites now read exactly like real instance members: `position.WithY(0f)`, and `position.HorizontalSqrMagnitude` is a genuine property read, not a disguised method call. A second block, `extension(Vector3)` (no receiver name), adds **static** extension members to `Vector3`'s own static surface: `Vector3.FromPolarXZ(45f, 2f)` reads as if `Vector3` shipped that factory method itself. Nothing about the underlying `Vector3` type changes -- this is still a static class doing the work under the hood -- but the call-site noise from `Before` is gone.

**JA:** `After~/StaticUtilitiesAfter.cs` は C# 14 の **extension member** -- 同じ static class の中の `extension(Vector3 v)` ブロック -- を使います。

```csharp
extension(Vector3 v)
{
    public float HorizontalSqrMagnitude => v.x * v.x + v.z * v.z;
    public Vector3 WithY(float y) => new(v.x, y, v.z);
}
```

呼び出し側は、本物のインスタンスメンバーとまったく同じように読めるようになります -- `position.WithY(0f)`、そして `position.HorizontalSqrMagnitude` は偽装されたメソッド呼び出しではなく、正真正銘のプロパティ読み取りです。もう 1 つのブロック、`extension(Vector3)`（受け手の名前なし）は、`Vector3` 自身の static な面に **static** extension member を追加します。`Vector3.FromPolarXZ(45f, 2f)` は、まるで `Vector3` がその factory メソッドを自ら備えているかのように読めます。`Vector3` という型自体は何も変わりません -- 裏側で実際に動いているのは相変わらず static class です -- ですが、`Before` にあった呼び出し側の雑音は消えます。

## Which Unity version/runtime this needs

**EN:**

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| Extension members (`extension(Vector3 v) { ... }`, instance and static) | **C# 14** | `no — needs Stage 2 swap` per `docs/feature-matrix.md`'s Stage 0 findings (bundled Roslyn 4.10 tops out at `-langversion 12`, plus `preview`/`latest`/`latestmajor`) | **Unity 6.8** (official), or Stage 2 (`docs/unity-lab-setup.md`) unofficially, today |

`docs/feature-matrix.md` records this row as `Working` on Mono once compiled -- so once a langversion bump makes it compile (Stage 2 today, or natively on 6.8), it behaves correctly at runtime; the gap is purely in what Unity's bundled Roslyn accepts.

**JA:**

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| extension member（`extension(Vector3 v) { ... }`、instance と static の両方） | **C# 14** | `docs/feature-matrix.md` の Stage 0 の結果によれば `no — needs Stage 2 swap`（同梱 Roslyn 4.10 は `-langversion 12`、加えて `preview`/`latest`/`latestmajor` が上限） | 今日の時点では **Unity 6.8**（公式）、または非公式に Stage 2（`docs/unity-lab-setup.md`） |

`docs/feature-matrix.md` はこの行を、コンパイルさえ通れば Mono 上で `Working` と記録しています -- つまり langversion の引き上げでコンパイルが通るようになれば（今日なら Stage 2、あるいは 6.8 ではネイティブに）、runtime での挙動は正しく、gap は純粋に Unity 同梱の Roslyn が何を受け付けるかだけの話です。

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix (see "Extension members (C# 14)").
- `../../../../docs/unity-lab-setup.md` -- Stage 2 setup (section 4), the unofficial way to reach this today.
