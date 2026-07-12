# 03_state_machines

**EN:** `Before/StateMachineBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/StateMachineAfter.cs` is the outlier among these six topics: it does not need Unity 6.8 or a Stage 2 Roslyn swap at all -- see "Which Unity version/runtime this needs" below. Its folder name still ends in `~`, keeping it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix.

**JA:** `Before/StateMachineBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/StateMachineAfter.cs` はこの 6 topic の中で例外的な存在です -- Unity 6.8 も Stage 2 の Roslyn 差し替えも、そもそも必要ありません。詳細は下の「どの Unity バージョン/ランタイムが必要か」を参照してください。それでもフォルダ名の末尾は `~` のままにしてあり、Unity の importer からは見えず、今の project を壊すことはありません。機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The C# 9 Unity idiom for a small state machine is an enum field plus nested `switch` statements, each branch ending in `break`:

```csharp
switch (_state)
{
    case PlayerState.Idle:
        switch (trigger)
        {
            case PlayerTrigger.Move:
                next = PlayerState.Running;
                break;
            default:
                next = _state;
                break;
        }
        break;
    // ...
}
```

`Before/StateMachineBefore.cs` also detects a fixed input combo (dash into jump) with explicit array-index comparisons. Both work, but the transition table's actual shape -- "this state plus this trigger goes to that state" -- is buried inside control flow, and every branch has to remember its own `break`.

**JA:** 小さな state machine 向けの C# 9 の Unity イディオムは、enum field とネストした `switch` 文で、各分岐が `break` で終わる形です。

```csharp
switch (_state)
{
    case PlayerState.Idle:
        switch (trigger)
        {
            case PlayerTrigger.Move:
                next = PlayerState.Running;
                break;
            default:
                next = _state;
                break;
        }
        break;
    // ...
}
```

`Before/StateMachineBefore.cs` は、固定の入力コンボ（ダッシュからジャンプ）の検出も、明示的な配列インデックスの比較で行っています。どちらも動きますが、遷移表の本当の形 -- 「この state とこの trigger なら、あの state へ」-- は制御フローの中に埋もれてしまい、すべての分岐が自分で `break` を覚えておく必要があります。

## Why the After form is better

**EN:** `After~/StateMachineAfter.cs` turns the whole nested-switch tree into one switch expression over a `(state, trigger)` tuple:

```csharp
var next = (_state, trigger) switch
{
    (PlayerState.Idle, PlayerTrigger.Move) => PlayerState.Running,
    (PlayerState.Running, PlayerTrigger.Jump) => PlayerState.Jumping,
    (PlayerState.Running, PlayerTrigger.Stop) => PlayerState.Idle,
    (PlayerState.Jumping, PlayerTrigger.Land) => PlayerState.Idle,
    _ => _state,
};
```

Every row is exactly the transition it represents, in one line, with no `break` to forget and no risk of accidental fallthrough. The combo check becomes a list pattern: `inputs is [PlayerTrigger.Move, PlayerTrigger.Jump, ..]` reads directly as "starts with Move, then Jump, then anything," instead of manual `Length`/index checks.

**JA:** `After~/StateMachineAfter.cs` は、ネストした switch の木全体を、`(state, trigger)` の tuple に対する 1 つの switch 式にします。

```csharp
var next = (_state, trigger) switch
{
    (PlayerState.Idle, PlayerTrigger.Move) => PlayerState.Running,
    (PlayerState.Running, PlayerTrigger.Jump) => PlayerState.Jumping,
    (PlayerState.Running, PlayerTrigger.Stop) => PlayerState.Idle,
    (PlayerState.Jumping, PlayerTrigger.Land) => PlayerState.Idle,
    _ => _state,
};
```

どの行も、それが表す遷移そのものが 1 行で書かれており、忘れがちな `break` も、意図しない fallthrough の危険もありません。コンボの判定は list パターンになります。`inputs is [PlayerTrigger.Move, PlayerTrigger.Jump, ..]` は「Move の次に Jump、その後は何でもよい」という形をそのまま読めます -- 手動の `Length` / インデックスチェックの代わりです。

## Which Unity version/runtime this needs

**EN:** Unlike most of these six topics, `After~/StateMachineAfter.cs` does not depend on C# 14 or Unity 6.8 at all:

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| Tuple pattern in a switch expression (`(_state, trigger) switch { ... }`) | C# 8 | Yes | Nothing extra -- already legal under Unity's default C# 9 |
| List pattern (`inputs is [PlayerTrigger.Move, PlayerTrigger.Jump, ..]`) | C# 11 | `expected — accepts ≤ 12 (Stage 1)` per `docs/feature-matrix.md`'s Stage 0 findings | A Stage 1 `csc.rsp` bump (`-langversion:preview` or higher), unofficially, today; official on Unity 6.8 |

Everything this file uses is reachable today via Stage 1 (`docs/unity-lab-setup.md`) -- it does not need Stage 2 or Unity 6.8's runtime at all. It still lives in `After~` rather than `After` purely to keep the six topics' shape consistent, and to keep `Assets/Contrasts` itself free of any langversion dependency by design (that experimentation belongs in `Assets/Lab`, not here).

**JA:** この 6 topic の大半と違い、`After~/StateMachineAfter.cs` は C# 14 にも Unity 6.8 にも、そもそも依存していません。

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| switch 式の中の tuple パターン（`(_state, trigger) switch { ... }`） | C# 8 | 可能 | 追加なし -- Unity の既定 C# 9 の下ですでに合法 |
| list パターン（`inputs is [PlayerTrigger.Move, PlayerTrigger.Jump, ..]`） | C# 11 | `docs/feature-matrix.md` の Stage 0 の結果によれば `expected — accepts ≤ 12 (Stage 1)` | 今日は非公式に Stage 1 の `csc.rsp` 引き上げ（`-langversion:preview` 以上）。Unity 6.8 では公式 |

このファイルが使っているものはすべて、今日 Stage 1（`docs/unity-lab-setup.md`）だけで届きます -- Stage 2 も Unity 6.8 の runtime も必要ありません。それでもこれが `After` ではなく `After~` に置かれているのは、純粋に 6 topic の形を揃えるためと、`Assets/Contrasts` 自体を設計上どの langversion 依存からも自由に保つためです（そうした実験は `Assets/Lab` の役割であり、ここではありません）。

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix (see "List patterns (C# 11)").
- `../../../../docs/unity-lab-setup.md` -- Stage 1 setup (section 3), if you want to try this today.
