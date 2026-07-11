// [C# 12] Primary constructors on classes
//
// EN: C# 12 lets a class declare its constructor parameters directly
//     in the type header: `class Foo(int bar)`. Those parameters are
//     usable anywhere in the class body — no hand-written private
//     field, no constructor body that does nothing but
//     `this.bar = bar;`. It's the boilerplate you've written a
//     hundred times, gone.
// JA: C# 12 ではクラスのコンストラクタパラメータを型ヘッダーに直接
//     書けます: `class Foo(int bar)`。このパラメータはクラス本体の
//     どこからでも使えます。手書きの private field も、`this.bar =
//     bar;` としか書かれていないコンストラクタ本体も不要です。何百回も
//     書いてきた定型コードが消えます。
//
// Unity note:
// EN: MonoBehaviours can't really use this — Unity instantiates them
//     itself (Instantiate / AddComponent), never `new`, so a
//     constructor parameter would have nothing to pass it. But plain
//     C# classes you DO `new` up yourself — a save-file service, an
//     inventory calculator, anything injected into a MonoBehaviour
//     rather than being one — are exactly where this removes the
//     boilerplate you're used to writing by hand.
// JA: MonoBehaviour ではこれをほぼ活用できません。Unity は
//     MonoBehaviour を Instantiate や AddComponent で自ら生成し、
//     `new` することはないため、コンストラクタパラメータに渡すものが
//     ないからです。しかし、自分で `new` するただの C# クラス —
//     セーブファイルサービス、インベントリの計算クラスなど、
//     MonoBehaviour 自身ではなく MonoBehaviour へ注入されるようなもの
//     — こそ、手で書いてきた定型コードを消せる場面です。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#primary-constructors

var inventory = new InventoryService(2);
Console.WriteLine(inventory.TryAdd());
Console.WriteLine(inventory.TryAdd());
Console.WriteLine(inventory.TryAdd());
Console.WriteLine(inventory.ItemCount);

class InventoryService(???)
{
    public int Capacity => capacity;
    public int ItemCount { get; private set; }

    public bool TryAdd()
    {
        if (ItemCount >= capacity) return false;
        ItemCount++;
        return true;
    }
}

// HINT EN: ??? is a primary constructor parameter list — same syntax
//          as an ordinary method parameter (`type name`). The class
//          body below already references `capacity` as if it existed.
// HINT JA: ??? はプライマリコンストラクタのパラメータリストです。普通の
//          メソッドの引数（`type name`）と同じ構文です。下のクラス本体
//          はすでに `capacity` が存在するものとして参照しています。
//
// EXPECTED OUTPUT:
// True
// True
// False
// 2
