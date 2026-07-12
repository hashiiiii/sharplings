# sharplings

[English](./README.md) | [日本語](./README_JA.md)

Unity エンジニアが最新 C#（10〜14）を ziglings 形式で学ぶための、C# 9 で足止めされている人向けの repo。

## 1. What this is

sharplings は、C# 10 から C# 14 で追加された言語機能を学ぶための、ziglings 形式の演習コースです。対象は、日々の仕事が C# 9 —— Unity の現行デフォルト `LangVersion` と、.NET Standard 2.1 の API —— で止まっている Unity エンジニアです。各演習は 1 つの壊れた `.cs` ファイルです。コンパイルが通らないか、通っても出力が期待と違います。直して runner でチェックし、次へ進みます。全 9 章 47 exercises、すべて gate 検証済みです。commit された時点で必ず失敗すること、EN/JA の解説・EN/JA の hint・runner が stdout を diff する `EXPECTED OUTPUT` ブロックを備えていることを確認済みです。

このプロジェクトは [ziglings](https://ziglings.org/) からインスピレーションを受けています——「1 つの壊れたファイルを直し、チェッカーを走らせ、繰り返す」という同じワークフローです——が、独立したプロジェクトであり、ziglings プロジェクトと提携・公認関係にあるわけではありません。

## 2. Why .NET 10 and not Unity (yet)

結論から言うと、Unity は C# 14 をまだ実行できません。しばらくの間は、です。

- Unity 6.7 alpha（6000.7.0a2）は実験的な CoreCLR **Desktop Player** を搭載しますが、スクリプト言語は **C# 9** のまま、API は **.NET Standard 2.1** のままです。ここでの CoreCLR は新しい player 実行基盤であり、新しいコンパイラではありません。
- **C# 14** と **.NET 10** の toolchain が来るのは **Unity 6.8**（Mono 廃止）からです。
- 情報源: [CoreCLR, Scripting, and Serialization Update - June 2026](https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299)、[Path to CoreCLR, 2026: Upgrade Guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279)。

つまり、今 C# 10〜14 を実際に学ぶ一番早い方法は、素の .NET 10 console 環境です——エディタの再コンパイルも、ドメインリロードも、次の Unity beta 待ちもありません。この repo の `exercises/` はまさにそれで、高速で使い捨てできる file-based な .NET 10 アプリです。`unity/` 側（6 節）は、同じ機能が実際の Unity project に着地する場所です——公式には 6.8 が来たときですが、Lab の `csc.rsp` の裏技を使えば C# ≤ 12 構文は今日すでにそこで動きます。

## 3. Setup

1. pin された .NET 10 SDK を install します。この repo は [mise](https://mise.jdx.dev/) を使います（`mise.toml` が `dotnet = "10.0.301"` を pin）。mise が入っていれば:

   ```
   mise install
   ```

   .NET 10.0.3xx の SDK であればどれでも動きます。`global.json` が正確な version と `rollForward: latestFeature` を pin しているため、SDK resolver は手元にある最も近い 10.0.3xx patch を受け入れます。

2. 確認:

   ```
   dotnet --version
   ```

   repo root で実行すると、`10.0.301`（または手元の 10.0.3xx patch version）が表示されるはずです。

## 4. How to work

リポジトリのルートから:

```
dotnet run --project tools/runner
```

runner は `exercises/` を順番に走査し、最初に通らない exercise で止まって hint を表示します。そのファイルを直して同じコマンドを再実行してください。合格済みの exercise は content hash で cache される（7 節）ため、再実行では実際に変更した箇所だけが再チェックされます。

各 exercise ファイルは自己完結しています: 機能の EN + JA 解説、Unity のイディオムへつなげる Unity note、docs へのリンク、壊れたコード本体（`???` プレースホルダ、素直なコンパイルエラー、あるいは微妙に間違った実装のいずれか）、EN + JA の `HINT`、そして runner がプログラムの stdout と diff する `EXPECTED OUTPUT` ブロックです。

解答はこの repo に意図的に含まれていません。詰まったら、この順で進めてください。

1. `EXPECTED OUTPUT` を読み直す —— たいてい何が間違っているかがそこで特定できます。
2. `HINT` を読み直す。
3. `Docs:` リンクをたどる。
4. AI に聞く —— 最後の手段であって、最初の手段ではありません。

## 5. Course map

| 章 | Exercises 数 | C# / .NET | 内容 |
|---|---|---|---|
| `00_intro` | 3 | .NET 10 | file-based apps（`dotnet run file.cs`）、top-level statements の復習 |
| `01_records` | 6 | C# 10, 12 | record struct、primary constructor |
| `02_patterns` | 6 | C# 10, 11 | 拡張されたプロパティパターン、list / slice パターン |
| `03_collections` | 6 | C# 12, 13 | collection expression + spread、params collections |
| `04_strings` | 5 | C# 10, 11 | 文字列補間の改善、raw string、UTF-8 literal |
| `05_types` | 7 | C# 10-14 | file-scoped namespace、required members、`field` キーワード |
| `06_generics` | 4 | C# 11 | generic math、static abstract members —— runtime 依存（6 節参照。今日の Unity の Mono runtime では動きません） |
| `07_performance` | 6 | C# 12-14 | `Span<T>`、inline array、`ref struct` の進化 |
| `08_extensions` | 4 | C# 14 | extension members + capstone exercise |

全 47 exercises。`--verify-broken` で検証済み: 違反 0 件 —— すべての exercise が commit された時点で失敗し、hint と expected-output ブロックの両方を備えています。

## 6. Unity side

Unity project は `unity/` に存在します（Unity 6000.7.0a2、Universal template）。Unity 側の入り口となるのは、次の 2 つの docs です。

- `docs/unity-lab-setup.md` —— 今日の Unity 内で最新 C# 構文をコンパイルできるようにする、非公式の裏技 lab の setup 手順。
- `docs/feature-matrix.md` —— どの言語機能がどこで実際に動くか（Mono editor / CoreCLR player / 将来の 6.8）を記録し続ける表。Lab の実験結果で更新されていきます。

`Assets/` 配下には、次の 2 つの zone が実在します。

- **`Contrasts/<topic>/`** —— 裏技不要の公式な Before/After 対。今日時点で 6 つの study があります。`Before/` には C# 9 の Unity イディオム（今日の 6.7 でコンパイル可）、`After~/` には最新 C# での書き換えが入ります。末尾の `~` に注目してください。`~` で終わるフォルダ・ファイルは Unity のコンパイルパイプラインから見えません（Unity 自身が隠しフォルダに使うのと同じ規約）。そのため `After~/` は project 内に存在していても、今日の build には影響しません。Unity 6.8 で C# 14 対応が来たら、`After~/` を `After/` に rename するだけで、そのまま有効になります。
- **`Lab/`** —— 独立した `asmdef` とローカルの `csc.rsp` を持つ実験ゾーン。`unity-lab-setup.md` に書かれた裏技を使って、Unity 6.7 の公式サポート範囲を超えて試すための場所です。

Stage 0（同梱 Roslyn コンパイラの probe）と Stage 1（Unity の build pipeline が assembly 単位の `csc.rsp` 言語バージョン上書きを実際に尊重することの確認）は、どちらも 2026-07-12 に実行済みです。要点: Unity の build pipeline は csc.rsp の言語バージョン上書きを尊重します——C# 11/12 構文は、今日すでに Lab の中でコンパイルできます。C# 14 にはまだ Unity 6.8 か、Stage 2 の Roslyn 差し替えが必要です。詳細は `docs/feature-matrix.md`（「Stage 1 results」）と `docs/unity-lab-setup.md` を参照してください。

## 7. Runner reference

リポジトリのルートから実行してください —— `--project` は現在のディレクトリからの相対パスとして解決されるため、下のコマンドはそこから実行した場合にのみそのまま動きます:

```
dotnet run --project tools/runner [--root <path>] [--no-cache] [--verify-broken]
```

- `--root <path>` —— repo root の自動検出を上書きします。既定では、runner は現在のディレクトリから `exercises/` フォルダが見つかるまで親へ遡ります。
- `--no-cache` —— 合格 cache を無視し、すべての exercise を最初からチェックし直します。
- `--verify-broken` —— 品質ゲートモード: すべての exercise が commit された時点で壊れており、必要な注釈（`EXPECTED OUTPUT`、`HINT EN`、`HINT JA` すべて）が揃っているかを検証します。最初の問題で止まらず、見つかった違反をすべて報告します。CI が実行するのはこのモードです。

**Cache file:** `.sharplings-cache.json`。repo root に置かれ、gitignore 対象です。各 exercise の SHA-256 content hash を key にしているため、合格済み exercise を編集すると、その cache entry は自動的に無効になります。

各 exercise は 10 秒の timeout の下で実行されます（無限ループを検知するためです）。

**Exit code:**

- `0` —— 通常モード: 全 exercise が合格。`--verify-broken`: 違反 0 件。
- `1` —— 通常モード: 最初に失敗した exercise で停止。`--verify-broken`: 1 件以上の違反あり。
- `2` —— usage エラー（未知の flag、または値なしの `--root`）。
