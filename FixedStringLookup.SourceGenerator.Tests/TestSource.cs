using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VisualFA;
namespace Tests
{
 
    partial class TestSource
    {
        [FixedStringLookup(new string[] {"foobar","foo","bar","baz","fubar"})]
        internal static partial bool IsFoo(string @string);
		[FixedStringLookup(new string[] { "abstract", "as", "ascending", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "descending", "do", "double", "dynamic", "else", "enum", "equals", "explicit", "extern", "event", "false", "finally", "fixed", "float", "for", "foreach", "get", "global", "goto", "if", "implicit", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "set", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void", "volatile", "while", "yield" })]
		public static partial bool IsKeyword(string identifier);
	}
}
}
