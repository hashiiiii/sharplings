// [C# 14] nameof with unbound generic types
//
// EN: `nameof` has always been the refactor-safe alternative to typing
//     a name as a string literal — rename the symbol and `nameof`
//     updates with it, where a literal silently goes stale. Generic
//     types were the exception: `nameof(List<int>)` worked, because
//     `List<int>` is a closed, fully-specified type, but a generic
//     helper that just wants the family name — "List", regardless of
//     what it's a list of — had no `nameof` to reach for, short of
//     picking an arbitrary type argument to close it with. C# 14 lets
//     `nameof` take an unbound generic type directly:
//     `nameof(List<>)` evaluates to `"List"`, no type argument needed.
// JA: `nameof` は、名前を文字列リテラルとして書くことに対する、リファ
//     クタリングに安全な代替手段であり続けてきました。シンボルの名前
//     を変更すれば `nameof` もそれに追従しますが、リテラルは気付かれ
//     ないまま古いままになります。ジェネリック型だけは例外でした。
//     `nameof(List<int>)` は動きます。`List<int>` が型引数まで確定し
//     た閉じた型だからです。しかし「何のリストか」に関わらず「List」
//     という総称名だけがほしいジェネリックなヘルパーには、適当な型引
//     数を選んで閉じるという妥協なしに使える `nameof` がありませんで
//     した。C# 14 では `nameof` に unbound generic type を直接渡せる
//     ようになりました: `nameof(List<>)` は型引数なしで `"List"` に
//     評価されます。
//
// Unity note:
// EN: A generic validation or logging helper — "warn me if any
//     `List<T>` I hand off is empty," regardless of `T` — wants to
//     name the container type in its message without hardcoding
//     `"List"` as a string literal that a rename would silently break.
//     `nameof(List<>)` gives it that name for free.
// JA: 「渡された `List<T>` が空なら、`T` が何であっても警告する」と
//     いった、ジェネリックなバリデーションやログ出力のヘルパーは、コン
//     テナ型の名前をメッセージに含めたいものですが、その名前をリネー
//     ムで気付かれずに壊れる文字列リテラル `"List"` としてハードコー
//     ドしたくはありません。`nameof(List<>)` を使えば、その名前をただ
//     で手に入れられます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#unbound-generic-types-and-nameof

static void Warn<T>(List<T> items) =>
    Console.WriteLine($"{nameof(???)} holds {items.Count} item(s)");

Warn(new List<int> { 1, 2, 3 });
Warn(new List<string> { "a" });

// HINT EN: ??? is the unbound generic type this method already works
//          with — the same one used in its own parameter type,
//          `List<T>`, written with the type parameter left empty.
// HINT JA: ??? はこのメソッドがすでに扱っている unbound generic type
//          です。自身の引数の型 `List<T>` と同じ型で、型引数の部分を
//          空のまま書いてください。
//
// EXPECTED OUTPUT:
// List holds 3 item(s)
// List holds 1 item(s)
