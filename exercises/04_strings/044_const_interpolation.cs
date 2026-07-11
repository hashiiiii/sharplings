// [C# 10] Const interpolated strings
//
// EN: A `const string` is baked directly into the calling code at
//     compile time (handy for switch-case labels, attribute arguments,
//     and zero-overhead lookups) — but before C# 10, building one from
//     other string constants meant repeating each literal or falling
//     back to `string.Concat`. C# 10 allows a `const string` to be
//     initialized with string interpolation (`$"..."`), as long as
//     every interpolated expression is itself a compile-time constant
//     string.
// JA: `const string` はコンパイル時に呼び出し側のコードへ直接埋め込ま
//     れます（switch-case のラベルや attribute の引数、オーバーヘッド
//     ゼロの参照などに便利です）— しかし C# 10 より前は、他の文字列定
//     数から組み立てようとするとリテラルを重複して書くか
//     `string.Concat` に頼るしかありませんでした。C# 10 では、
//     interpolation 対象のすべての式がコンパイル時定数の文字列である
//     限り、`const string` を string interpolation（`$"..."`）で初期
//     化できるようになりました。
//
// Unity note:
// EN: Addressables address keys, PlayerPrefs key names, and analytics
//     event-name constants are often assembled from a shared prefix
//     plus a per-feature suffix. Keeping the pieces `const` and
//     combining them with `$"..."` avoids retyping (and mistyping) the
//     full string everywhere it is used, while the result is still a
//     genuine compile-time constant — usable anywhere a plain string
//     literal would be.
// JA: Addressables のアドレスキーや PlayerPrefs のキー名、アナリティ
//     クスのイベント名の定数は、共通の prefix と機能ごとの suffix を
//     組み合わせて作ることがよくあります。各パーツを `const` のまま
//     `$"..."` で結合すれば、使用箇所ごとに完全な文字列を書き直す
//     （そして書き間違える）手間がなくなり、しかも結果は本物のコンパ
//     イル時定数のままです — 通常の文字列リテラルが使える場所ならど
//     こでも使えます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated

const string studio = "Capcom";
string gameName = "Sharplings";
const string fullTitle = $"{studio} - {gameName}";

Console.WriteLine(fullTitle);

// HINT EN: `fullTitle` is declared `const`, so every interpolated
//          expression inside it must itself be a compile-time constant
//          string. `gameName` is a plain (non-const) local variable,
//          which is why the compiler rejects it. Add `const` to the
//          `gameName` declaration.
// HINT JA: `fullTitle` は `const` として宣言されているため、その中の
//          すべての interpolation 式もコンパイル時定数の文字列でなけ
//          ればなりません。`gameName` は普通の（const でない）ローカ
//          ル変数なので、コンパイラに拒否されます。`gameName` の宣言
//          に `const` を追加してください。
//
// EXPECTED OUTPUT:
// Capcom - Sharplings
