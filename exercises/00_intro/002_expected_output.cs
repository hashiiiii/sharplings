// [.NET 10] How the runner checks your output
//
// EN: The runner does not analyze or judge your code's logic. It runs
//     the file with `dotnet run` and compares everything written to
//     stdout against the EXPECTED OUTPUT block at the bottom of this
//     file, line by line (trailing whitespace and line-ending
//     differences are ignored, but the text itself must match
//     exactly). If a file compiles and runs but prints the wrong
//     text, the verdict is OutputMismatch — a different failure from
//     a compile error.
// JA: runner はコードのロジックを解析したり評価したりはしません。
//     `dotnet run` でファイルを実行し、標準出力をこのファイル末尾の
//     EXPECTED OUTPUT block と 1 行ずつ比較します（行末の空白や改行
//     コードの違いは無視されますが、文字列そのものは完全に一致する
//     必要があります）。コンパイル・実行はできても出力する文字列が
//     違う場合、判定はコンパイルエラーとは別の OutputMismatch に
//     なります。
//
// Unity note:
// EN: This is the same shape as a failing Unity Test Runner assertion
//     (e.g. Assert.AreEqual(expected, actual)) — the code compiles and
//     runs fine, but the value it produces is not the one the test
//     expects.
// JA: これは Unity の Test Runner でアサーション（例:
//     Assert.AreEqual(expected, actual)）が失敗する状況と同じ形です。
//     コードはコンパイル・実行できますが、生成される値がテストの期待値
//     と一致していません。
//
// Docs: https://learn.microsoft.com/dotnet/core/tools/dotnet-run

Console.WriteLine("sharplings checks stdout, not your source code.");
Console.WriteLine("This line is deliberately wrong.");

// HINT EN: Only the second line is wrong. Compare it character by
//          character with the second line of EXPECTED OUTPUT below.
// HINT JA: 間違っているのは 2 行目だけです。下の EXPECTED OUTPUT の
//          2 行目と 1 文字ずつ比較してください。
//
// EXPECTED OUTPUT:
// sharplings checks stdout, not your source code.
// This line matches once you fix it.
