// [C# 11] Expressions inside interpolation holes
//
// EN: Before C# 11, the expression inside an interpolation hole
//     (`{...}`) had to fit on a single line — spreading a `switch`
//     expression, or any other multi-line construct, across several
//     lines inside `{...}` was a syntax error. C# 11 lifts that
//     restriction: any C# expression, including one with its own
//     newlines, is allowed inside an interpolation hole, which lets a
//     multi-branch calculation read like ordinary formatted code
//     instead of being squeezed onto one line or extracted into a
//     separate method.
// JA: C# 11 より前は、interpolation hole（`{...}`）の中に書く式は 1
//     行に収める必要があり、`switch` 式のような複数行にまたがる構文を
//     `{...}` の中に書くと構文エラーになっていました。C# 11 はこの制
//     限を取り除き、改行を含む式であっても interpolation hole の中に
//     書けるようになりました。これにより、複数分岐の計算を 1 行に詰め
//     込んだり別メソッドへ切り出したりせず、通常の整形されたコードの
//     ように読める形で書けます。
//
// Unity note:
// EN: Debug overlays and damage-popup text often need a one-line
//     HP-to-status mapping (`"HP 35: Wounded"`). Writing the `switch`
//     expression directly inside the interpolation hole keeps the
//     label text and the mapping logic in the same statement,
//     formatted the same way a standalone `switch` expression would
//     be, instead of splitting it into a helper method just to keep
//     the string literal short.
// JA: デバッグオーバーレイやダメージポップアップのテキストでは、HP を
//     ステータス文字列にマッピングする処理を 1 行で表現したいことがよ
//     くあります（`"HP 35: Wounded"`）。`switch` 式を interpolation
//     hole の中に直接書けば、ラベルのテキストとマッピングのロジックを
//     同じ 1 つの文の中に保ちつつ、通常の独立した `switch` 式と同じよ
//     うに整形できます — 文字列リテラルを短く保つためだけにヘルパー
//     メソッドへ分割する必要はありません。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated#structure-of-an-interpolated-string

int hp = 35;

string status = $"HP {hp}: {
    hp switch
    {
        <= 0 => "Dead",
        < 30 => "Critical",
        < 70 => ???,
        _ => "Healthy"
    }
}";

Console.WriteLine(status);

// HINT EN: Fill in the string this branch of the `switch` expression
//          should produce when `hp` is at least 30 but less than 70.
//          Check EXPECTED OUTPUT below for the exact word and
//          capitalization.
// HINT JA: `hp` が 30 以上 70 未満のときに、この `switch` 式の分岐が返
//          すべき文字列を埋めてください。正確な単語と大文字・小文字は
//          下の EXPECTED OUTPUT を確認してください。
//
// EXPECTED OUTPUT:
// HP 35: Wounded
