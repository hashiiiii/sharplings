// [C# 11] File-local types
//
// EN: Every type in this course's exercise files — `Program`,
//     `SaveSlot`, `Character`, `PlayerProfile` — is declared with no
//     access modifier at all, which defaults to `internal`: visible
//     anywhere in the same assembly. That's harmless here because each
//     exercise runs as its own single-file app, its own assembly, with
//     nothing else around to collide with. It stops being harmless the
//     moment many files share one assembly: two files each declaring
//     `internal class Formatter` is a duplicate-definition error, not
//     two separate, unrelated helpers. C# 11's `file` modifier fixes
//     that by construction: `file class Formatter` is visible only in
//     the file that declares it, so any number of files can each have
//     their own same-named helper with zero risk of collision.
// JA: このコースの exercise ファイルに出てくる型 — `Program` や
//     `SaveSlot`、`Character`、`PlayerProfile` — はどれもアクセス修飾
//     子を書いておらず、デフォルトの `internal`（同じアセンブリ内どこ
//     からでも見える）になっています。ここでは無害です。各 exercise は
//     それぞれ独立した単一ファイルアプリ、つまり独立したアセンブリと
//     して実行され、周りに衝突する相手がいないからです。無害でなくな
//     るのは、多数のファイルが 1 つのアセンブリを共有する瞬間です。
//     2 つのファイルがそれぞれ `internal class Formatter` を宣言する
//     と、無関係な 2 つのヘルパーではなく、定義の重複エラーになりま
//     す。C# 11 の `file` 修飾子はこれを構造的に解決します。
//     `file class Formatter` はそれを宣言したファイルの中だけで見え
//     るため、いくつのファイルがそれぞれ同名のヘルパーを持っていても
//     衝突する心配がありません。
//
// Unity note:
// EN: Unity compiles every script inside one assembly definition
//     (asmdef) — or, with no asmdef at all, the whole `Assets` folder —
//     as a single assembly, exactly the scenario above. Two feature
//     folders that each add their own internal `Utils` or `Helper`
//     class hit CS0101 ("already contains a definition") the moment
//     both land in the same asmdef, even though neither author knew
//     the other's helper existed. Marking those helpers `file` removes
//     the collision without renaming either one.
// JA: Unity はアセンブリ定義（asmdef）1 つにつき、あるいは asmdef が
//     まったくない場合は `Assets` フォルダ全体を、1 つのアセンブリと
//     してコンパイルします。まさに上で説明した状況です。それぞれの
//     フィーチャーフォルダが独自の internal な `Utils` や `Helper`
//     クラスを追加していると、両者が同じ asmdef に入った瞬間に
//     CS0101（「定義が重複しています」）が発生します。どちらの作者も
//     相手のヘルパーの存在を知らなかったとしてもです。それらのヘル
//     パーに `file` を付ければ、どちらの名前も変えずに衝突を解消でき
//     ます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/file

Console.WriteLine(Formatter.Describe(7));
Console.WriteLine(Formatter.Describe(42));

??? class Formatter
{
    public static string Describe(int level) => $"Lv.{level}";
}

// HINT EN: ??? is the C# 11 modifier that makes `Formatter` visible
//          only inside this file — the same guarantee `internal`
//          gives within an assembly, narrowed down to a single file.
// HINT JA: ??? は `Formatter` をこのファイルの中だけで見えるようにする
//          C# 11 の修飾子です。`internal` がアセンブリ内で与える保証
//          を、ファイル 1 つの範囲までさらに絞り込んだものです。
//
// EXPECTED OUTPUT:
// Lv.7
// Lv.42
