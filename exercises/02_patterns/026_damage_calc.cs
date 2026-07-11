// [C# 9] Combined and/or/not patterns
//
// EN: A quick applied review: relational patterns (`> 0`, `>= 100`)
//     combine with the logical pattern keywords `and`, `or`, and `not`
//     to express a range — or an exception to one — directly in a
//     pattern. `>= 40 and not >= 100` reads as "40 or more, but not
//     100 or more," i.e. the half-open range [40, 100). Precedence
//     matches boolean logic: `not` binds tighter than `and`, which
//     binds tighter than `or`.
// JA: 短い応用復習です。関係パターン（`> 0`、`>= 100`）は論理パターンの
//     キーワード `and`、`or`、`not` と組み合わさり、範囲や「その例外」を
//     パターンの中で直接表現できます。`>= 40 and not >= 100` は
//     「40 以上、ただし 100 以上は除く」、つまり半開区間 [40, 100) と
//     読めます。優先順位は bool の論理演算と同じで、`not` は `and`
//     より強く結合し、`and` は `or` より強く結合します。
//
// Unity note:
// EN: A damage-tier classifier for hit reactions and VFX selection —
//     graze, hit, heavy hit, critical — is exactly the kind of
//     `if (dmg > 0 && dmg < 10) ... else if (dmg >= 10 && ...) ...`
//     ladder every combat system grows over time. The switch
//     expression turns that ladder into one readable table, but only
//     if every boundary between tiers is exactly right.
// JA: 被弾リアクションや VFX 選択のためのダメージ階級判定 — かすり、
//     ヒット、強打、会心の一撃 — はまさに、どんな戦闘システムでも
//     育っていく `if (dmg > 0 && dmg < 10) ... else if (dmg >= 10 &&
//     ...) ...` の連鎖そのものです。switch 式はその連鎖を 1 つの
//     読みやすいテーブルに変えてくれますが、それは各階級の境界が
//     すべて正確な場合に限られます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#logical-patterns

int[] damages = [0, 5, 15, 45, 100, 150];

foreach (var damage in damages)
    Console.WriteLine(Classify(damage));

static string Classify(int damage) => damage switch
{
    0 => "miss",
    > 0 and < 10 => "graze",
    >= 10 and < 40 => "hit",
    >= 40 and not > 100 => "heavy hit",
    >= 100 => "critical",
    _ => "unknown"
};

// HINT EN: The "heavy hit" tier should stop just below 100, not
//          include it — compare its `not` clause with the arm right
//          below it, `>= 100`. Which relational operator makes the
//          two ranges meet exactly at the boundary without
//          overlapping?
// HINT JA: "heavy hit" の階級は 100 のすぐ手前で止まるべきで、100 自体は
//          含みません。この arm の `not` 節を、すぐ下の arm
//          `>= 100` と見比べてください。2 つの範囲が重ならず、境界
//          ぴったりで接するのは、どちらの関係演算子でしょうか。
//
// EXPECTED OUTPUT:
// miss
// graze
// hit
// heavy hit
// critical
// critical
