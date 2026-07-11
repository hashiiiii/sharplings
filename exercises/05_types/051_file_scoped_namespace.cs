// [C# 10] File-scoped namespace declarations
//
// EN: Before C# 10, putting types inside a namespace meant wrapping
//     them in a block: `namespace GameData { ... }`, indenting every
//     member one level for as long as the file exists. C# 10 lets you
//     declare the namespace once, with a semicolon instead of braces:
//     `namespace GameData;`. Everything below that line belongs to the
//     namespace, with no block to close and one less indent level
//     throughout. The trade-off: a file-scoped namespace declaration
//     must be the only namespace in the file and must precede every
//     other member — including the Main method that top-level
//     statements compile down to. A file gets file-scoped namespace
//     syntax or top-level statements, never both, in either order.
//     That's why the code below declares an explicit `Main` instead of
//     using the top-level-statement style every other exercise in this
//     course uses.
// JA: C# 10 より前は、型を namespace に入れるにはブロックで囲む必要が
//     ありました: `namespace GameData { ... }`。ファイルが存在する限り
//     すべてのメンバーが 1 段階分インデントされます。C# 10 では、波括弧
//     の代わりにセミコロンを使い namespace を 1 回だけ宣言できます:
//     `namespace GameData;`。この行より下のものはすべてその namespace
//     に属し、閉じるブロックがないためインデントが全体で 1 段階減りま
//     す。ただし引き換えに制約があります。file-scoped namespace 宣言は
//     ファイル内で唯一の namespace 宣言でなければならず、top-level
//     statements がコンパイルされてできる Main メソッドを含む、ファイル
//     内の他のすべてのメンバーより前になければなりません。1 つのファイ
//     ルは file-scoped namespace 構文か top-level statements のどちら
//     かしか持てず、順序を入れ替えても両方は持てません。このコースの
//     他の exercise がすべて top-level statements の書き方を使っている
//     のに、下のコードだけ明示的な `Main` を宣言しているのはこのためです。
//
// Unity note:
// EN: Unity's generated .csproj historically pinned an older C#
//     language version than the Editor's own Roslyn could actually
//     run, so a script using `namespace GameData;` could compile fine
//     from the command line while Rider or Visual Studio's IntelliSense
//     still underlined it as an error — a version-pinning gap, not a
//     real one. Teams have filed that exact mismatch as an editor bug
//     before finding the fix: a `csc.rsp` file bumping `-langversion`.
//     Check docs/feature-matrix.md for which Unity/IDE combination
//     needs that override before assuming your syntax is wrong.
// JA: Unity が生成する .csproj は、Editor 自身の Roslyn が実際に実行
//     できるバージョンより古い C# 言語バージョンを固定していることが
//     歴史的にありました。そのため `namespace GameData;` を使うスクリ
//     プトがコマンドラインからは問題なくコンパイルできるのに、Rider や
//     Visual Studio の IntelliSense ではエラーの波線が引かれる、という
//     ことが起こります — これはバージョン固定によるずれであり、本当の
//     エラーではありません。チームがこの不一致をそのままエディタのバグ
//     として報告し、後になって `csc.rsp` ファイルで `-langversion` を
//     引き上げるのが解決策だと気づく、ということが実際に起きています。
//     自分のコードが間違っていると決めつける前に、docs/feature-matrix.md
//     でどの Unity/IDE の組み合わせにこの上書きが必要かを確認してくだ
//     さい。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/namespace

namespace GameData ???

public class Program
{
    public static void Main()
    {
        var slot = new SaveSlot("Aria", 42);
        Console.WriteLine(slot.Describe());
    }
}

public class SaveSlot(string name, int level)
{
    public string Describe() => $"{name} (Lv.{level})";
}

// HINT EN: Replace ??? with a semicolon: `namespace GameData;`
//          declares the namespace for the rest of the file, with no
//          braces and no extra indentation.
// HINT JA: ??? をセミコロンに置き換えてください。`namespace GameData;`
//          はファイルの残り全体に適用される namespace 宣言で、波括弧
//          も追加のインデントも不要です。
//
// EXPECTED OUTPUT:
// Aria (Lv.42)
