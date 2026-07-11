// [C# 9] Fixture: throws at runtime
Console.WriteLine("before");
throw new InvalidOperationException("boom");
// HINT EN: none
// HINT JA: なし
// EXPECTED OUTPUT:
// before
