// [C# 9] Switch expressions over tuples as a state machine
//
// EN: Nothing new syntactically here — this applies switch
//     expressions and tuple patterns, both already familiar, to a
//     shape you'll use constantly: a finite state machine. The classic
//     `if (state == X && trigger == Y) state = Z; else if (...)` ladder
//     becomes a single switch expression matching a `(state, trigger)`
//     tuple against a transition table, one arm per row, with a
//     wildcard `_` arm that leaves the state unchanged for any
//     combination the table doesn't list.
// JA: 構文としては新しいものはありません。すでに知っている switch 式と
//     tuple パターンを、よく使う形 — 有限状態機械（FSM） — に当てはめる
//     回です。おなじみの `if (state == X && trigger == Y) state = Z;
//     else if (...)` の連鎖は、`(state, trigger)` という tuple を
//     遷移テーブルと照合する 1 つの switch 式になります。テーブルの
//     1 行が 1 つの arm に対応し、テーブルに載っていない組み合わせは
//     ワイルドカードの `_` arm で状態を変えずに素通しします。
//
// Unity note:
// EN: This is the textbook Animator/FSM pattern in plain C#: a
//     PlayerState enum, an input trigger, and a transition table.
//     Unity's own Animator Controller is a visual editor for exactly
//     this table. Writing it as code like this is what you'd do for a
//     headless simulation, a unit test, or logic you want to drive
//     without the Animator window open at all.
// JA: これは、素の C# で書いた教科書的な Animator/FSM パターンです。
//     PlayerState という enum、入力トリガー、そして遷移テーブル。
//     Unity 自身の Animator Controller は、まさにこのテーブルを
//     ビジュアルに編集するエディタです。これをコードとして書くのは、
//     ヘッドレスなシミュレーションやユニットテスト、あるいは Animator
//     ウィンドウを開かずにロジックを駆動したい場面で使う書き方です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression

var state = PlayerState.Idle;
Trigger[] triggers = [Trigger.Move, Trigger.Jump, Trigger.Land, Trigger.Stop];

foreach (var trigger in triggers)
{
    state = Transition(state, trigger);
    Console.WriteLine(state);
}

static PlayerState Transition(PlayerState state, Trigger trigger) => (state, trigger) switch
{
    (PlayerState.Idle, Trigger.Move) => PlayerState.Walking,
    (PlayerState.Walking, Trigger.Stop) => PlayerState.Idle,
    (PlayerState.Walking, Trigger.Jump) => PlayerState.Walking,
    (PlayerState.Idle, Trigger.Jump) => PlayerState.Jumping,
    (PlayerState.Jumping, Trigger.Land) => PlayerState.Idle,
    _ => state
};

enum PlayerState { Idle, Walking, Jumping }
enum Trigger { Move, Stop, Jump, Land }

// HINT EN: The (Walking, Jump) row of the transition table maps to
//          the wrong state — compare it with the (Idle, Jump) row
//          right below it, which correctly transitions to Jumping.
//          Walking should behave the same way when Jump fires.
// HINT JA: 遷移テーブルの (Walking, Jump) の行が、間違った状態に
//          マッピングされています。すぐ下の (Idle, Jump) の行と
//          比べてみてください。そちらは正しく Jumping へ遷移します。
//          Walking も Jump が発生したときは同じように振る舞うべき
//          です。
//
// EXPECTED OUTPUT:
// Walking
// Jumping
// Idle
// Idle
