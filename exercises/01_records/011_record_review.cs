// [C# 9] Records and non-destructive mutation
//
// EN: A quick warm-up on ground you already half-know. A `record` is a
//     reference type built for carrying data: the compiler generates
//     value equality (two records with equal properties are `==`),
//     a readable ToString(), and — the part this exercise reviews —
//     the `with` expression, which copies a record and changes only
//     the properties you name, leaving the original untouched.
// JA: 半分知っている内容の復習です。`record` はデータを運ぶために作られた
//     参照型で、コンパイラが値の等価性（プロパティが等しい 2 つの record
//     は `==` になります）、読みやすい ToString()、そしてこの exercise が
//     復習する `with` 式を自動生成します。`with` 式は record をコピーし、
//     指定したプロパティだけを変更します。元の record 自体は変更されません。
//
// Unity note:
// EN: Records shipped in C# 9, but most Unity code never adopted them —
//     Unity's serializer doesn't know what to do with a positional
//     record or an init-only property, so anything the Inspector needs
//     to show or save is still a plain mutable class or [Serializable]
//     struct. Records mostly live in pure C# logic that Unity never
//     serializes: save-game snapshots, network DTOs, calculation
//     results you build and discard.
// JA: record は C# 9 で登場しましたが、ほとんどの Unity コードでは採用
//     されませんでした。Unity のシリアライザは positional record や
//     init-only プロパティの扱い方を知らないため、Inspector に表示・保存
//     が必要なものは今も普通の mutable な class か [Serializable] な
//     struct のままです。record が活躍するのは Unity がシリアライズしない
//     純粋な C# ロジック（セーブデータのスナップショット、ネットワーク
//     DTO、使い捨ての計算結果など）です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation

var hero = new PlayerStats("Aria", 1, 100);
var leveledUp = hero ??? { Level = 2, Health = 120 };

Console.WriteLine(hero);
Console.WriteLine(leveledUp);
Console.WriteLine(hero == leveledUp);

record PlayerStats(string Name, int Level, int Health);

// HINT EN: Replace ??? with the C# 9 keyword that copies a record and
//          applies the changes in the braces, without mutating `hero`.
// HINT JA: ??? を、record をコピーして波括弧内の変更を適用する C# 9 の
//          キーワードに置き換えてください。`hero` 自体は変更されません。
//
// EXPECTED OUTPUT:
// PlayerStats { Name = Aria, Level = 1, Health = 100 }
// PlayerStats { Name = Aria, Level = 2, Health = 120 }
// False
