// [C# 11] IParsable<T> and generic parsing
//
// EN: Parsing is the other half of the generic-math BCL story: before
//     C# 11 there was no interface that meant "`T` has a static `Parse`
//     method," so a generic helper could not call `T.Parse(text)` for
//     an arbitrary numeric (or otherwise parsable) `T` — only concrete
//     types like `int.Parse` or `double.Parse` could be called
//     directly, one hardcoded type at a time. `IParsable<T>`, built on
//     the same static abstract members as `INumber<T>`, requires
//     exactly one static method: `static T Parse(string,
//     IFormatProvider?)` (plus a non-throwing `TryParse`). Every BCL
//     numeric type implements it — `DateTime` and `Guid` do too — and
//     any generic method written against `where T : IParsable<T>` can
//     call `T.Parse(...)` once and have it work for whichever `T` the
//     caller closes it with.
// JA: パースは generic math の BCL 側のもう半分です。C# 11 より前は、
//     「`T` は静的な `Parse` メソッドを持つ」を意味する interface が
//     存在しなかったため、ジェネリックなヘルパーは任意の数値型（あるい
//     はその他のパース可能な型）である `T` に対して `T.Parse(text)` を
//     呼び出せませんでした。`int.Parse` や `double.Parse` のような具体
//     的な型だけを、1 つずつハードコードして直接呼び出すしかありません
//     でした。`IParsable<T>` は `INumber<T>` と同じ static abstract メ
//     ンバーの上に構築されており、静的メソッドをちょうど 1 つだけ要求
//     します: `static T Parse(string, IFormatProvider?)`（と、例外を
//     投げない `TryParse`）。BCL のすべての数値型がこれを実装しており、
//     `DateTime` や `Guid` も実装しています。`where T : IParsable<T>`
//     に対して書かれたジェネリックメソッドは、`T.Parse(...)` を 1 度
//     呼び出すだけで、呼び出し側がどの `T` で閉じても動作します。
//
// Unity note:
// EN: `IParsable<T>`'s `Parse` / `TryParse` are themselves static
//     abstract interface members, so the same Mono limitation from the
//     first exercise in this chapter applies here: a generic helper
//     written against `where T : IParsable<T>` does not run on Unity's
//     current Mono / IL2CPP runtime at all — only once Unity ships the
//     CoreCLR-based runtime in 6.8. See docs/feature-matrix.md.
// JA: `IParsable<T>` の `Parse` / `TryParse` はそれ自体が static
//     abstract interface メンバーであるため、この章の最初の exercise
//     で見た Mono の制約がここにも当てはまります。`where T :
//     IParsable<T>` に対して書かれたジェネリックなヘルパーは、Unity の
//     現行の Mono / IL2CPP ランタイムではまったく動作しません。Unity が
//     CoreCLR ベースのランタイムを 6.8 で出荷して初めて動くようになり
//     ます。詳しくは docs/feature-matrix.md を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/api/system.iparsable-1

static T ReadDamage<T>(string raw) where T : IParsable<T> =>
    T.Parse(raw[1..], null);

Console.WriteLine(ReadDamage<int>("42"));
Console.WriteLine(ReadDamage<long>("1700"));

// HINT EN: ReadDamage slices off the first character of raw before
//          parsing (`raw[1..]`) — leftover from an older log format
//          that prefixed every value with a sign character. These
//          plain damage strings do not have one, so the first digit
//          gets silently dropped instead. Parse `raw` itself, not
//          `raw[1..]`.
// HINT JA: ReadDamage はパースする前に raw の先頭 1 文字を切り落として
//          います（`raw[1..]`）— これは、すべての値に符号文字を前置し
//          ていた古いログ形式の名残です。この単純なダメージ文字列には
//          符号がないため、代わりに先頭の桁が黙って失われてしまいま
//          す。`raw[1..]` ではなく `raw` そのものをパースしてください。
//
// EXPECTED OUTPUT:
// 42
// 1700
