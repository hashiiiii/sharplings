// [C# 11] IParsable<T> and generic parsing
//
// EN: Parsing is the other half of the generic-math BCL story: before
//     C# 11 there was no interface that meant "`T` has a static `Parse`
//     method," so a generic helper could not call `T.Parse(text)` for
//     an arbitrary numeric (or otherwise parsable) `T`. The workaround
//     was to special-case inside the generic method — compare
//     `typeof(T)` against each supported type, call that type's own
//     concrete `Parse` (`int.Parse`, `double.Parse`, ...), cast the
//     result back to `T`, and fall back to something arbitrary for
//     every type the ladder forgot. `IParsable<T>`, built on the same
//     static abstract members as `INumber<T>`, requires exactly one
//     static method: `static T Parse(string, IFormatProvider?)` (plus
//     a non-throwing `TryParse`). Every BCL numeric type implements it
//     — `DateTime` and `Guid` do too — and any generic method written
//     against `where T : IParsable<T>` can call `T.Parse(...)` once
//     and have it work for whichever `T` the caller closes it with.
// JA: パースは generic math の BCL 側のもう半分です。C# 11 より前は、
//     「`T` は静的な `Parse` メソッドを持つ」を意味する interface が
//     存在しなかったため、ジェネリックなヘルパーは任意の数値型（あるい
//     はその他のパース可能な型）である `T` に対して `T.Parse(text)` を
//     呼び出せませんでした。回避策はジェネリックメソッドの内側での
//     特別扱いでした。`typeof(T)` をサポートする型と 1 つずつ比較し、
//     その型自身の具体的な `Parse`（`int.Parse`、`double.Parse`、...）
//     を呼び、結果を `T` へキャストで戻し、連鎖が忘れた型には適当な
//     フォールバックを返す、というものです。`IParsable<T>` は
//     `INumber<T>` と同じ static abstract メンバーの上に構築されて
//     おり、静的メソッドをちょうど 1 つだけ要求します: `static T
//     Parse(string, IFormatProvider?)`（と、例外を投げない
//     `TryParse`）。BCL のすべての数値型がこれを実装しており、
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

static T ReadDamage<T>(string raw)
{
    if (typeof(T) == typeof(int))
        return (T)(object)int.Parse(raw);
    return default!;
}

Console.WriteLine(ReadDamage<int>("42"));
Console.WriteLine(ReadDamage<long>("1700"));

// HINT EN: ReadDamage genuinely parses only one closed type — any
//          other `T` falls through to its type's default value, which
//          is why the second line prints 0. The Docs page above
//          describes the interface whose constraint lets a generic
//          method call the static `Parse` of `T` itself. Constrain `T`
//          to it, and the whole typeof-and-cast ladder collapses into
//          a single call that works for every parsable type.
// HINT JA: ReadDamage が本当にパースできるのは 1 つの閉じた型だけで、
//          それ以外の `T` はすべて型のデフォルト値に落ちてしまいます。
//          2 行目が 0 と出力されるのはそのためです。上の Docs ページ
//          は、制約を付けることでジェネリックメソッドが `T` 自身の
//          静的な `Parse` を呼び出せるようになる interface を説明して
//          います。`T` をその interface で制約すれば、typeof とキャス
//          トの連鎖全体が、パース可能なすべての型で動く 1 つの呼び出
//          しにまとまります。
//
// EXPECTED OUTPUT:
// 42
// 1700
