// [C# 11] UTF-8 string literals
//
// EN: A `string` in .NET is always UTF-16 internally — every `char` is
//     2 bytes. Sending text over a network socket or writing it to a
//     file usually wants UTF-8 instead, which historically meant
//     calling `Encoding.UTF8.GetBytes(...)` at runtime. C# 11's `u8`
//     suffix (`"..."u8`) asks the compiler to encode the literal as
//     UTF-8 bytes directly, producing a `ReadOnlySpan<byte>` with zero
//     runtime encoding cost.
// JA: .NET の `string` は内部的に常に UTF-16 であり、`char` 1 つが 2
//     バイトです。ネットワークソケットへの送信やファイルへの書き込み
//     では通常 UTF-8 が欲しくなり、従来は実行時に
//     `Encoding.UTF8.GetBytes(...)` を呼ぶ必要がありました。C# 11 の
//     `u8` サフィックス（`"..."u8`）はコンパイラにリテラルを直接
//     UTF-8 バイト列としてエンコードさせ、実行時のエンコードコストな
//     しで `ReadOnlySpan<byte>` を生成します。
//
// Unity note:
// EN: Network payloads (matchmaking pings, lightweight RPC headers)
//     and asset-bundle manifests often need raw UTF-8 bytes, not a
//     `string`. `u8` literals skip the runtime
//     `Encoding.UTF8.GetBytes` call entirely for any text known at
//     compile time — handy for protocol constants. Note also how byte
//     count and character count diverge for non-ASCII text like
//     Japanese: each hiragana character costs 3 UTF-8 bytes, not 1.
// JA: マッチメイキングの ping や軽量な RPC ヘッダー、アセットバンドル
//     のマニフェストなどでは、`string` ではなく生の UTF-8 バイト列が
//     必要になる場面がよくあります。`u8` リテラルはコンパイル時にわか
//     っているテキストについて、実行時の `Encoding.UTF8.GetBytes` 呼
//     び出しを完全に省略します — プロトコル定数に便利です。また非
//     ASCII テキスト（日本語など）ではバイト数と文字数が一致しない点
//     にも注目してください — ひらがな 1 文字は 1 バイトではなく 3
//     UTF-8 バイトを消費します。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#utf-8-string-literals

string message = "こんにちは、Unity";
ReadOnlySpan<byte> utf8 = ???;

Console.WriteLine(message.Length);
Console.WriteLine(utf8.Length);

// HINT EN: Add the `u8` suffix to the same text as `message` above:
//          `"こんにちは、Unity"u8`. `.Length` on a `string` counts
//          UTF-16 code units (characters); `.Length` on the resulting
//          `ReadOnlySpan<byte>` counts encoded bytes — the two numbers
//          will not match for non-ASCII text.
// HINT JA: 上の `message` と同じテキストに `u8` サフィックスを付けま
//          す: `"こんにちは、Unity"u8`。`string` の `.Length` は
//          UTF-16 のコード単位（文字数）を数え、結果の
//          `ReadOnlySpan<byte>` の `.Length` はエンコード後のバイト数
//          を数えます — 非 ASCII テキストではこの 2 つの数は一致しま
//          せん。
//
// EXPECTED OUTPUT:
// 11
// 23
