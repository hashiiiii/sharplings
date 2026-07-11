// [C# 14] Null-conditional assignment
//
// EN: Before C# 14, `?.` could only appear on the right side of an
//     assignment — reading through a possibly-null reference safely
//     was fine (`var n = enemy?.Name;`), but *writing* through one
//     still needed an explicit `if`: `if (enemy is not null) { enemy
//     .Name = "Boss"; }`. C# 14 allows `?.` (and `?[]`) on the left
//     side too: `enemy?.Name = "Boss";` is exactly that `if` block,
//     collapsed to one line. If `enemy` is null, the whole statement
//     is a no-op — the right-hand side isn't even evaluated — and if
//     it isn't null, the assignment happens normally. Compound
//     assignment (`+=`, `-=`, ...) works the same way; `++`/`--` do
//     not, since there's no meaningful "no-op" reading for those.
// JA: C# 14 より前、`?.` は代入の右側にしか書けませんでした — null かも
//     しれない参照を安全に *読む* のは問題ありませんでした
//     （`var n = enemy?.Name;`）。しかしそこへ *書き込む* には、まだ
//     明示的な `if` が必要でした。`if (enemy is not null) { enemy.Name
//     = "Boss"; }` のようにです。C# 14 は左側にも `?.`（および `?[]`）
//     を書けるようにします。`enemy?.Name = "Boss";` は、まさにその
//     `if` ブロックを 1 行に畳んだものです。`enemy` が null なら、この
//     文全体が何もしません — 右側は評価すらされません — null でなければ
//     普通どおり代入が起こります。複合代入（`+=`、`-=` など）も同じ
//     ように動きますが、`++`/`--` は対象外です。これらには意味のある
//     「no-op」の読み方が存在しないためです。
//
// Unity note:
// EN: This looks like a strict win for the `if (target != null)
//     target.Something = value;` chains that fill Unity scripts — but
//     there is exactly one place it can silently change behavior:
//     `UnityEngine.Object` (and everything derived from it —
//     `MonoBehaviour`, `GameObject`, `Component`, ...) overloads `==`
//     and `!=` so a *destroyed* object still compares equal to `null`,
//     even though the underlying C# reference is not actually null —
//     this is Unity's well-known "fake null". `if (enemy == null)`
//     goes through that overload and correctly treats a destroyed
//     enemy as null. `enemy?.Name = "Boss"` does not — `?.`'s null
//     check is a raw reference test, bypassing any user-defined `==`
//     entirely. So a destroyed-but-not-truly-null `UnityEngine.Object`
//     sails straight through `?.` and the assignment runs anyway. Keep
//     using explicit `== null`/`!= null` checks for `UnityEngine.Object`
//     types; reserve `?.` (and this new assignment form) for plain C#
//     reference types. See docs/feature-matrix.md.
// JA: これは Unity スクリプトを埋め尽くす `if (target != null)
//     target.Something = value;` の連鎖にとって、一見まったくの改善に
//     見えます — ですがただ 1 か所だけ、挙動を静かに変えてしまう場所が
//     あります。`UnityEngine.Object`（そしてそこから派生するすべて —
//     `MonoBehaviour`、`GameObject`、`Component` など）は `==` と `!=`
//     をオーバーロードしており、*破棄済み* のオブジェクトは、内部の
//     C# 参照が実際には null でなくても null と等しく比較されます —
//     これが Unity でよく知られた「fake null」です。`if (enemy ==
//     null)` はこのオーバーロードを経由するため、破棄済みの enemy を
//     正しく null 扱いします。しかし `enemy?.Name = "Boss"` はそう
//     なりません — `?.` の null チェックは生の参照テストであり、
//     ユーザー定義の `==` を完全に迂回します。そのため、破棄済みだが
//     本当には null でない `UnityEngine.Object` は `?.` をそのまま
//     素通りし、代入は実行されてしまいます。`UnityEngine.Object` 系の
//     型には明示的な `== null` / `!= null` チェックを使い続け、`?.`
//     （およびこの新しい代入の形）は素の C# 参照型のために取っておいて
//     ください。詳細は docs/feature-matrix.md を参照してください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-

Enemy? active = new Enemy { Name = "Slime" };
Enemy? destroyed = null;

??? = "Boss Slime";
destroyed?.Name = "Ghost";

Console.WriteLine(active?.Name);
Console.WriteLine(destroyed is null);

class Enemy
{
    public string Name { get; set; } = "";
}

// HINT EN: `destroyed?.Name = "Ghost";` right below already shows the
//          shape: null-conditional member access on the left of `=`.
//          Write the same shape for `active`, whose name is `active`
//          and whose member is `Name`.
// HINT JA: すぐ下の `destroyed?.Name = "Ghost";` がすでにその形を示して
//          います — `=` の左側に置いた null 条件付きメンバーアクセス
//          です。`active` についても同じ形を書いてください。変数名は
//          `active`、メンバーは `Name` です。
//
// EXPECTED OUTPUT:
// Boss Slime
// True
