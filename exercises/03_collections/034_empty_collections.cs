// [C# 12] The empty collection literal
//
// EN: `[]` is a collection expression too — the empty one. Target-typed
//     the same way as any other bracket literal, it becomes whatever
//     empty collection the declared or return type calls for: an empty
//     array, an empty `List<T>`, and so on. In C# 9 you reached for
//     `Array.Empty<T>()` (which, unlike `new T[0]`, returns a cached,
//     shared instance, so no allocation happens on the array path) or
//     `new List<T>()`. For an array-typed target, `[]` compiles down to
//     that same `Array.Empty<T>()` call, so you get the same
//     allocation-free behavior with less to type. What it should never
//     be replaced with is a sentinel: an array holding a placeholder
//     value to mean "nothing here."
// JA: `[]` も collection expression の一種です — 空のものです。他の
//     角括弧リテラルと同じくターゲット型に応じて構築され、宣言または
//     戻り値の型が求める空のコレクション（空の配列、空の `List<T>`
//     など）になります。C# 9 では `Array.Empty<T>()`（`new T[0]` と
//     違い、共有されるキャッシュ済みインスタンスを返すためアロケー
//     ションが発生しません）や `new List<T>()` を使っていました。
//     配列型がターゲットの場合、`[]` はコンパイル後に同じ
//     `Array.Empty<T>()` 呼び出しになるため、書く量を減らしながら
//     アロケーションフリーな挙動を得られます。絶対に置き換えては
//     いけないのは、「ここには何もない」を表すためにプレースホルダー
//     値を 1 つ入れた配列（センチネル）です。
//
// Unity note:
// EN: Older inventory / loot helpers often signal "nothing dropped"
//     with a sentinel array like `new int[] { -1 }` instead of a true
//     empty collection, forcing every caller to special-case that
//     magic value. `GetSecretLoot` below has exactly that bug: swap
//     the sentinel for `[]` and callers can go back to trusting
//     `.Length == 0`.
// JA: 古いインベントリ／loot 用のヘルパーは、「何もドロップしなかった」
//     ことを、本当に空のコレクションではなく `new int[] { -1 }` の
//     ようなセンチネル配列で表すことがよくあり、呼び出し側は全員
//     そのマジックナンバーを特別扱いしなければなりません。下の
//     `GetSecretLoot` にはまさにこのバグがあります。センチネルを
//     `[]` に置き換えれば、呼び出し側は再び `.Length == 0` を
//     信頼できるようになります。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions

static int[] GetSecretLoot(int floor)
{
    if (floor % 10 == 0)
        return [500, 1000];

    return new int[] { -1 };
}

int[] floor3 = GetSecretLoot(3);
int[] floor10 = GetSecretLoot(10);

Console.WriteLine(floor3.Length);
Console.WriteLine(floor10.Length);
Console.WriteLine(floor3.Length + floor10.Length);

// HINT EN: Floor 3 is not a bonus floor, so `GetSecretLoot(3)` should
//          report zero items — it currently reports one, because
//          `new int[] { -1 }` is a one-element sentinel array, not an
//          empty collection. Replace that line with the empty
//          collection literal.
// HINT JA: 3 階はボーナス階ではないため、`GetSecretLoot(3)` はアイテム
//          0 個を報告すべきですが、現在は 1 個と報告されます。
//          `new int[] { -1 }` が要素 1 個のセンチネル配列であり、
//          空のコレクションではないためです。その行を空のコレクション
//          リテラルに置き換えてください。
//
// EXPECTED OUTPUT:
// 0
// 2
// 2
