### FixedStringLookup.SourceGenerator

A C# source generator for creating fast fixed string lookup tables for a set of predetermined strings known at compile time.

Usage:

```cs
partial class Lookups 
{
    [FixedStringLookup(new string[] {"foobar","foo","bar","baz","fubar"})]
    internal static partial bool IsFoo(string @string);
}
```
After which you can do
```cs
Console.WriteLine(Lookups.IsFoo("bar"));
```

For strings with a decent spread of lengths, it's about as fast as a hashtable, but takes less memory.