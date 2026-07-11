// [C# 10] Positional deconstruction
//
// EN: Any positional record — `record class` or `record struct` alike
//     — automatically gets a `Deconstruct` method matching its
//     primary constructor parameters. That lets you unpack an
//     instance into separate variables in one line, the same way you
//     would unpack a tuple: `var (x, y) = point;`.
// JA: positional record は `record class` でも `record struct` でも、
//     プライマリコンストラクタのパラメータに対応する `Deconstruct`
//     メソッドが自動的に生成されます。これにより、tuple を展開するのと
//     同じ書き方 `var (x, y) = point;` で、インスタンスを個別の変数へ
//     1 行で分解できます。
//
// Unity note:
// EN: This is the plain-C# version of a habit you already have.
//     Unity added its own `Deconstruct` extension methods to Vector2
//     and Vector3 a while back specifically so `var (x, y) = transform
//     .position;` works. Here it's automatic for any positional
//     record you declare — no hand-written extension method required.
// JA: これは、すでに身についている習慣の素の C# 版です。Unity は以前、
//     `var (x, y) = transform.position;` を書けるようにするため、
//     Vector2 と Vector3 に独自の `Deconstruct` 拡張メソッドを追加しま
//     した。ここでは、自分で宣言した positional record であれば拡張
//     メソッドを手書きしなくても自動的に同じことができます。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct

Point spawn = new(3, 4);
var (x, ???) = spawn;

Console.WriteLine($"x={x}, y={y}");
Console.WriteLine($"distance from origin: {x * x + y * y}");

record struct Point(int X, int Y);

// HINT EN: The deconstruction pattern needs one designation per
//          positional parameter of `Point`, in order (X, then Y).
//          Replace ??? with the name used right below as `y`.
// HINT JA: この deconstruction パターンには `Point` の positional
//          パラメータ 1 つにつき 1 つの designation が、順番通り
//          （X、続いて Y）必要です。??? を、すぐ下で `y` として使われて
//          いる名前に置き換えてください。
//
// EXPECTED OUTPUT:
// x=3, y=4
// distance from origin: 25
