// [C# 11] Raw string literals
//
// EN: Before C# 11, embedding JSON, regex, or file paths in a string
//     meant escaping every `"` and `\` by hand — a maze of `\"` and
//     `\\` that is easy to get wrong, as the code below shows. C# 11
//     raw string literals (`"""..."""`, three or more double quotes)
//     treat everything between the delimiters as literal text: no
//     escape processing at all, so quotes and backslashes are written
//     exactly as they should appear in the output.
// JA: C# 11 より前は、文字列の中に JSON や正規表現、ファイルパスを埋め
//     込むには `"` と `\` をすべて手作業でエスケープする必要があり
//     （下のコードのように）、`\"` と `\\` が入り乱れて間違えやすいも
//     のでした。C# 11 の raw string literal（`"""..."""`、3 つ以上のダ
//     ブルクォート）はデリミタの間をすべてそのままの文字列として扱い
//     ます — エスケープ処理は一切行われないため、引用符やバックスラッ
//     シュは出力に現れてほしい形そのまま書けます。
//
// Unity note:
// EN: Unity code embeds JSON snippets and Windows-style asset paths
//     (`Assets\Prefabs\...`) in strings constantly — item catalogs,
//     save-data templates, shader property blocks. Raw string literals
//     remove the backslash-escaping tax entirely, so a pasted JSON
//     sample or a copied Windows path can go straight into the source
//     file unmodified.
// JA: Unity のコードでは JSON スニペットや Windows 形式のアセットパス
//     （`Assets\Prefabs\...`）を文字列に埋め込む場面が頻繁にあります —
//     アイテムカタログ、セーブデータのテンプレート、シェーダーのプロ
//     パティブロックなど。raw string literal を使えば逆スラッシュのエ
//     スケープという手間が完全になくなるので、貼り付けた JSON サンプ
//     ルやコピーした Windows パスをそのままソースファイルに書けます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#string-literals

string itemJson = "{\n  \"name\": \"Health Potion\",\n  \"prefabPath\": \"Assets\Prefabs\Potion.prefab\"\n}";

Console.WriteLine(itemJson);

// HINT EN: The illegal escapes are `\P` in `\Prefabs` and `\Potion` —
//          `\P` is not a recognized escape character (unlike `\n` or
//          `\"`, which are valid). Rewrite the whole string as a
//          triple-quoted raw string (`"""..."""`) instead: put the
//          JSON on its own indented lines, with the closing `"""`
//          lined up under the opening brace so its indentation is
//          what gets stripped from every line.
// HINT JA: 不正なエスケープは `\Prefabs` の `\P` と `\Potion` の `\P`
//          です — `\P` は（`\n` や `\"` とは違い）認識されるエスケー
//          プ文字ではありません。文字列全体をトリプルクォートの raw
//          string（`"""..."""`）で書き直しましょう。JSON をインデン
//          トした行に分けて書き、閉じる `"""` を開き括弧の位置に揃え
//          ると、その字下げ幅が各行から取り除かれます。
//
// EXPECTED OUTPUT:
// {
//   "name": "Health Potion",
//   "prefabPath": "Assets\Prefabs\Potion.prefab"
// }
