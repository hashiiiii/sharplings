// [C# 7.2] Span<T> and AsSpan slicing
//
// EN: The C# 9 Unity habit for "give me part of this string/array" was
//     `.Substring(start, length)` or LINQ's `.Skip(...).Take(...)` —
//     both hand back a brand-new heap object that is a *copy* of the
//     data, even though the original string or array already holds
//     every byte you wanted. `Span<T>` and `ReadOnlySpan<T>` (a
//     `ref struct` wrapping a pointer and a length, introduced
//     alongside C# 7.2) give you a *view* over existing memory
//     instead: `someString.AsSpan()` returns a `ReadOnlySpan<char>`
//     over the string's own backing storage, and `.Slice(start,
//     length)` returns another span over that same memory. No copy,
//     no new object — slicing a span is just arithmetic on a pointer
//     and a length.
// JA: C# 9 時代の Unity での「この文字列・配列の一部が欲しい」に対する
//     いつものやり方は `.Substring(start, length)` や LINQ の
//     `.Skip(...).Take(...)` でした — どちらも、元の文字列や配列が
//     すでに欲しいデータをすべて持っているにもかかわらず、データの
//     *コピー* である新しいヒープオブジェクトを返します。`Span<T>` と
//     `ReadOnlySpan<T>`（ポインタと長さを包む `ref struct` で、C# 7.2
//     と共に導入されました）は、既存のメモリへの *view* を代わりに
//     提供します。`someString.AsSpan()` は文字列自身のバッキングスト
//     レージ上の `ReadOnlySpan<char>` を返し、`.Slice(start, length)`
//     は同じメモリ上の別の span を返します。コピーも新しいオブジェクト
//     もありません — span のスライスはポインタと長さの計算に過ぎません。
//
// Unity note:
// EN: Unity's current Mono/IL2CPP runtime already ships `Span<T>` (it
//     has shipped since netstandard2.1 landed), so `.AsSpan()` and
//     `.Slice(...)` below compile and run in Unity projects unchanged
//     today — this is not something waiting on the 6.8/CoreCLR
//     transition. It matters most in exactly the place per-frame GC
//     pressure hides: an `Update()` that parses a per-frame HUD
//     string, a save-data field, or a log line with `.Substring()`
//     allocates fresh garbage every single frame it runs, which is
//     precisely the steady drip that causes frame-time spikes. What
//     *is* still catching up to 6.8 is coverage — some newer BCL
//     method overloads that accept a span as an argument (rather than
//     just producing one, as here) are missing from Unity's older BCL
//     surface; see docs/feature-matrix.md for specifics.
// JA: Unity の現行の Mono / IL2CPP ランタイムはすでに `Span<T>` を
//     出荷済みです（netstandard2.1 が来て以来ずっと）。そのため下の
//     `.AsSpan()` と `.Slice(...)` は今日の Unity プロジェクトでも
//     そのままコンパイル・実行できます — これは 6.8 / CoreCLR への
//     移行を待つものではありません。これはフレームごとの GC 負荷が
//     隠れているまさにその場所で効きます。`Update()` の中で毎フレーム
//     HUD 文字列やセーブデータのフィールド、ログ行を `.Substring()`
//     で解析すれば、実行するたびに新しいゴミが生まれます。これこそ
//     フレーム時間のスパイクを引き起こす、じわじわとした積み重ねです。
//     6.8 でまだ追いついていないのは「網羅性」の方です — span を
//     *受け取る*（ここでのように span を生み出すのではなく）新しい
//     BCL のオーバーロードの一部が、Unity の古い BCL 表面には
//     まだ存在しません。詳細は docs/feature-matrix.md を参照して
//     ください。
//
// Docs: https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.asspan

string status = "HP:87,MP:32";

ReadOnlySpan<char> span = status.AsSpan();
int commaIndex = span.IndexOf(',');

ReadOnlySpan<char> hpField = ???;
ReadOnlySpan<char> mpField = span.Slice(commaIndex + 1);

int hp = int.Parse(hpField.Slice(hpField.IndexOf(':') + 1));
int mp = int.Parse(mpField.Slice(mpField.IndexOf(':') + 1));

Console.WriteLine(hp);
Console.WriteLine(mp);

// HINT EN: `mpField` right below already shows the shape you need:
//          `span.Slice(commaIndex + 1)` takes everything from an
//          index onward. `hpField` needs everything *before* the
//          comma instead — the other `Slice` overload takes both a
//          start index and a length.
// HINT JA: すぐ下の `mpField` がすでに必要な形を示しています —
//          `span.Slice(commaIndex + 1)` はあるインデックス以降の
//          すべてを取り出します。`hpField` はその代わりに、カンマ
//          より *前* のすべてが必要です。もう一方の `Slice`
//          オーバーロードは開始インデックスと長さの両方を受け取ります。
//
// EXPECTED OUTPUT:
// 87
// 32
