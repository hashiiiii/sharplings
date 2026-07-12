# 04_coroutines_async

**EN:** `Before/CoroutinesAsyncBefore.cs` compiles today, in Unity 6000.7.0a2, at Unity's default `LangVersion` (C# 9) -- no `csc.rsp`, no Lab tricks. `After~/CoroutinesAsyncAfter.cs` is the other outlier among these six topics (alongside `03_state_machines`): it does not need a newer C# language version at all -- see "Which Unity version/runtime this needs" below. Its folder name still ends in `~`, keeping it invisible to Unity's importer, so it cannot break the project as it sits today. See `../../../../docs/feature-matrix.md` for the full per-feature compile matrix.

**JA:** `Before/CoroutinesAsyncBefore.cs` は今日の Unity 6000.7.0a2 で、Unity の既定 `LangVersion`（C# 9）のまま、`csc.rsp` も Lab の裏技も無しでコンパイルが通ります。`After~/CoroutinesAsyncAfter.cs` は（`03_state_machines` と並んで）この 6 topic のもう 1 つの例外です -- より新しい C# 言語バージョンに、そもそも依存していません。詳細は下の「どの Unity バージョン/ランタイムが必要か」を参照してください。それでもフォルダ名の末尾は `~` のままにしてあり、Unity の importer からは見えず、今の project を壊すことはありません。機能ごとの詳しい比較は `../../../../docs/feature-matrix.md` を参照してください。

## The idiom

**EN:** The classic Unity idiom for "do this, wait a second, do it again" is an `IEnumerator` coroutine, started with `StartCoroutine` and paused with `yield return new WaitForSeconds(...)`:

```csharp
private IEnumerator CountdownRoutine(int seconds)
{
    for (int i = seconds; i > 0; i--)
    {
        Debug.Log($"countdown: {i}");
        yield return new WaitForSeconds(1f);
    }

    Debug.Log("liftoff");
}
```

This works, and has worked since Unity's earliest versions, but it is not an ordinary C# method: it is a state machine driven externally by Unity's coroutine scheduler, it cannot return a value to its caller, and an exception thrown inside it does not propagate the way a normal method's would.

**JA:** 「これをやって、1 秒待って、またやる」ための古典的な Unity イディオムは、`StartCoroutine` で開始し、`yield return new WaitForSeconds(...)` で一時停止する `IEnumerator` コルーチンです。

```csharp
private IEnumerator CountdownRoutine(int seconds)
{
    for (int i = seconds; i > 0; i--)
    {
        Debug.Log($"countdown: {i}");
        yield return new WaitForSeconds(1f);
    }

    Debug.Log("liftoff");
}
```

これは動きますし、Unity の最初期のバージョンからずっと動いてきました。しかしこれは普通の C# メソッドではありません -- Unity のコルーチンスケジューラが外部から駆動する state machine であり、呼び出し元へ値を返すこともできず、内部で投げられた例外も普通のメソッドのようには伝播しません。

## Why the After form is better

**EN:** `After~/CoroutinesAsyncAfter.cs` replaces the coroutine with `async Awaitable` -- Unity 6's own awaitable type, purpose-built to replace `IEnumerator` coroutines for exactly this shape:

```csharp
private async Awaitable CountdownAsync(int seconds)
{
    for (int i = seconds; i > 0; i--)
    {
        Debug.Log($"countdown: {i}");
        await Awaitable.WaitForSecondsAsync(1f);
    }

    Debug.Log("liftoff");
}
```

This is now an ordinary async method: exceptions propagate through `await` the way they would from any other async call, `await` composes with other `Awaitable`s (and, via a bridge, with `Task`), and the compiler's own async/await machinery -- not a hand-rolled `IEnumerator` state machine -- drives it. `Run()`'s `_ = CountdownAsync(3);` mirrors `StartCoroutine(CountdownRoutine(3));` exactly: both are "fire and forget" from a synchronous `[ContextMenu]` method.

**JA:** `After~/CoroutinesAsyncAfter.cs` はコルーチンを `async Awaitable` -- Unity 6 自身の awaitable 型で、まさにこの形のために `IEnumerator` コルーチンを置き換える目的で作られたもの -- に置き換えます。

```csharp
private async Awaitable CountdownAsync(int seconds)
{
    for (int i = seconds; i > 0; i--)
    {
        Debug.Log($"countdown: {i}");
        await Awaitable.WaitForSecondsAsync(1f);
    }

    Debug.Log("liftoff");
}
```

これで、ごく普通の async メソッドになります -- 例外は他の async 呼び出しと同じように `await` を通じて伝播し、`await` は他の `Awaitable`（そして bridge 経由で `Task`）とも組み合わせられ、手書きの `IEnumerator` state machine ではなく、コンパイラ自身の async/await の仕組みがそれを駆動します。`Run()` の `_ = CountdownAsync(3);` は `StartCoroutine(CountdownRoutine(3));` と全く同じ形です -- どちらも、同期的な `[ContextMenu]` メソッドからの「fire and forget」です。

## Which Unity version/runtime this needs

**EN:** Unlike most of these six topics, the blocker here is not a C# language version at all:

| Feature used | Introduced | Compiles via `csc.rsp` today? | Needs |
|---|---|---|---|
| `async`/`await`, custom awaitables | C# 5 / 7 | Yes | Nothing extra -- long available under Unity's default C# 9 |
| `UnityEngine.Awaitable` and `Awaitable.WaitForSecondsAsync` | Unity 6.0 (2023.3) **API**, not a C# language feature | N/A -- this is a runtime type, not a language-version question | Nothing extra -- ships in `UnityEngine.CoreModule.dll` in this exact Unity 6000.7.0a2 install (confirmed locally: `Awaitable`, `AwaitableCompletionSource`, `WaitForSecondsAsync`, `NextFrameAsync` are all present in the assembly) |

This is genuinely the odd one out: `After~/CoroutinesAsyncAfter.cs` could be renamed to `After` (dropping the `~`) right now and it should compile as-is in this project -- verify locally if you try it, since this note has not been confirmed by actually running it inside `Assets/Contrasts` (it is kept in `After~` here purely so all six topics share the same folder shape, and so `Assets/Contrasts` stays free of any langversion/API assumptions by policy, not because `Awaitable` itself needs 6.8 or a Roslyn swap).

**JA:** この 6 topic の大半と違い、ここでの障壁はそもそも C# の言語バージョンではありません。

| 使っている機能 | 導入 | 今日 `csc.rsp` でコンパイルできるか | 必要なもの |
|---|---|---|---|
| `async`/`await`、カスタム awaitable | C# 5 / 7 | 可能 | 追加なし -- Unity の既定 C# 9 の下でずっと前から使えます |
| `UnityEngine.Awaitable` と `Awaitable.WaitForSecondsAsync` | Unity 6.0（2023.3）の **API** であり、C# の言語機能ではありません | 該当なし -- これは言語バージョンの話ではなく runtime の型の話です | 追加なし -- この正確な Unity 6000.7.0a2 install の `UnityEngine.CoreModule.dll` に同梱済みです（`Awaitable`、`AwaitableCompletionSource`、`WaitForSecondsAsync`、`NextFrameAsync` がすべて assembly 内に存在することをローカルで確認済み） |

これは本当に例外です -- `After~/CoroutinesAsyncAfter.cs` は今すぐ `After`（`~` を外すだけ）にリネームしても、この project でそのままコンパイルが通るはずです -- 試す場合はローカルで確認してください。実際に `Assets/Contrasts` の中で動かして確認したわけではありません（ここで `After~` に置いているのは、純粋に 6 topic すべてで同じフォルダの形を揃えるため、そして `Assets/Contrasts` を方針としてどの langversion / API の前提からも自由に保つためであり、`Awaitable` 自体が 6.8 や Roslyn 差し替えを必要とするからではありません）。

## Docs

- `../../../../docs/feature-matrix.md` -- the full per-feature compile/runtime matrix (this topic is API-driven, not covered by the language-feature rows there).
- `../../../../docs/unity-lab-setup.md` -- background on the Unity 6.7/6.8 scripting timeline this whole repo is built around.
