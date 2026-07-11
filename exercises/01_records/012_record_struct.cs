// [C# 10] record struct
//
// EN: C# 10 lets you put `record` in front of `struct` instead of
//     `class`. You get the same positional syntax, the same generated
//     ToString(), and the same generated `==` — but the type is a
//     value type: assigning one variable to another copies the whole
//     value, and mutating the copy never touches the original.
// JA: C# 10 では `class` の代わりに `struct` の前へ `record` を置けます。
//     positional 構文、生成される ToString()、生成される `==` はすべて
//     同じですが、この型は値型です。ある変数を別の変数へ代入すると値
//     全体がコピーされ、コピー側を変更しても元の値には影響しません。
//
// Unity note:
// EN: Gameplay code leans on value types for exactly this copy
//     behavior — Vector3, Quaternion, and your own per-frame structs
//     avoid GC allocations and avoid aliasing bugs. Before C# 10 you
//     had to hand-write Equals/GetHashCode/ToString on a struct to get
//     record-like ergonomics; record struct generates all of it.
// JA: ゲームプレイ用のコードは、まさにこのコピー挙動のために値型を使い
//     ます。Vector3、Quaternion、そして自作の毎フレーム使う struct は
//     GC アロケーションとエイリアシングのバグを避けます。C# 10 以前は
//     struct に record のような使い勝手を持たせるため Equals・
//     GetHashCode・ToString を手書きする必要がありましたが、record
//     struct はそれらをすべて自動生成します。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#record-struct

Vector2Int a = new(3, 4);
Vector2Int b = a;
b.X = 99;

Console.WriteLine(a);
Console.WriteLine(b);
Console.WriteLine(a == b);

??? Vector2Int(int X, int Y);

// HINT EN: ??? is two words: the same keyword pair from this file's
//          title line, in the same order, placed where `struct` alone
//          would otherwise go.
// HINT JA: ??? は 2 つの単語です。このファイルのタイトル行と同じ
//          キーワードの組を、同じ順序で `struct` 単独の代わりに
//          置いてください。
//
// EXPECTED OUTPUT:
// Vector2Int { X = 3, Y = 4 }
// Vector2Int { X = 99, Y = 4 }
// False
