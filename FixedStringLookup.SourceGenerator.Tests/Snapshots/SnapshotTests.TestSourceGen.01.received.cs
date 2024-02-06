//HintName: FixedStringLookup.Methods.g.cs
namespace Tests
{
    partial class TestSource
    {
        private static string[][] _IsFoo = new string[6][] {
            null,
            null,
            null,
            new string[] { "bar", "baz", "foo" },
            null,
            new string[] { "fubar" }
        };
        internal static partial bool IsFoo(string @string) => FixedStringLookup.SourceGenerator.Lookup.Contains(_IsFoo, @string);
        private static string[][] _IsKeyword = new string[10][] {
            null,
            null,
            new string[] { "as", "do", "if", "is" },
            new string[] { "for", "get", "int", "new", "out", "ref", "set", "try", "var" },
            new string[] { "base", "bool", "byte", "case", "char", "else", "enum", "goto", "lock", "long", "null", "this", "true", "uint", "void" },
            new string[] { "async", "await", "break", "catch", "class", "const", "event", "false", "fixed", "float", "sbyte", "short", "throw", "ulong", "using", "while", "yield" },
            new string[] { "double", "equals", "extern", "global", "object", "params", "public", "return", "sealed", "sizeof", "static", "string", "struct", "switch", "typeof", "unsafe", "ushort" },
            new string[] { "checked", "decimal", "default", "dynamic", "finally", "foreach", "partial", "private", "virtual" },
            new string[] { "abstract", "continue", "delegate", "explicit", "implicit", "internal", "operator", "override", "readonly", "volatile" },
            new string[] { "ascending", "interface", "namespace", "protected", "unchecked" }
        };
        public static partial bool IsKeyword(string @string) => FixedStringLookup.SourceGenerator.Lookup.Contains(_IsKeyword, @string);
    }

}

