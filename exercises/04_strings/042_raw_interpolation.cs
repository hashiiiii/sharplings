// [C# 11] Interpolated raw string literals
//
// EN: A raw string literal can also be interpolated, but the interplay
//     between `$` and `{` needs care: a JSON payload's own single
//     braces `{ }` must not be swallowed as interpolation holes.
//     Prefixing with two (or more) `$` characters — `$$"""..."""` —
//     changes the rule: single braces in the text are literal, and
//     only a matching number of braces (`{{expr}}` for `$$`) opens an
//     interpolation hole.
// JA: raw string literal も interpolation できますが、`$` と `{` の関
//     係には注意が必要です — JSON ペイロード自身が持つ単一の中括弧
//     `{ }` が interpolation hole として飲み込まれてはいけません。`$`
//     を 2 つ以上重ねて書く（`$$"""..."""`）と規則が変わります: テキ
//     スト中の単一の中括弧はそのままリテラルとして扱われ、`$$` と同
//     じ個数の中括弧（`{{expr}}`）だけが interpolation hole を開きま
//     す。
//
// Unity note:
// EN: Building a save-file or dialogue-line JSON template where most
//     of the structure is fixed but a couple of fields are
//     interpolated (player name, current level) is exactly this
//     shape. `$$"""..."""` lets you keep the JSON's own `{` `}`
//     untouched and only mark the few fields that come from variables.
// JA: セーブファイルやダイアログ行の JSON テンプレートを組み立てる際、
//     構造の大半は固定でいくつかのフィールドだけを埋め込みたい（プレ
//     イヤー名や現在のレベルなど）というのはまさにこの形です。
//     `$$"""..."""` を使えば JSON 自身の `{` `}` はそのままに、変数か
//     ら来る一部のフィールドだけを印付けできます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated#interpolated-raw-string-literals

string playerName = "Ryu";
int level = 7;

string saveJson = $$"""
    {
      "player": "{{playerName}}",
      "level": {{???}}
    }
    """;

Console.WriteLine(saveJson);

// HINT EN: Inside a `$$"""..."""` literal, an interpolation hole needs
//          doubled braces — `{{expr}}` — because a single `{` or `}`
//          is treated as literal text (that is what lets the JSON's
//          own braces pass through untouched above). Replace `???`
//          with the `level` variable, wrapped in one more pair of
//          braces, the same way `{{playerName}}` is written above it.
// HINT JA: `$$"""..."""` リテラルの中で interpolation hole を作るに
//          は、二重の中括弧 `{{expr}}` が必要です — 単一の `{` や `}`
//          はリテラルなテキストとして扱われるからです（それがすぐ上
//          の JSON 自身の中括弧をそのまま通している理由です）。`???`
//          を `level` 変数に置き換え、上の `{{playerName}}` と同じよ
//          うにもう一組の中括弧で包んでください。
//
// EXPECTED OUTPUT:
// {
//   "player": "Ryu",
//   "level": 7
// }
