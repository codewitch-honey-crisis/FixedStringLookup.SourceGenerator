using System.Diagnostics;
using System.Text;
var sa = new string[] { "abstract", "as", "ascending", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "descending", "do", "double", "dynamic", "else", "enum", "equals", "explicit", "extern", "event", "false", "finally", "fixed", "float", "for", "foreach", "get", "global", "goto", "if", "implicit", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "set", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void", "volatile", "while", "yield" };
var hashset = new HashSet<string>(sa);
var sw = new Stopwatch();

for (int pass = 1; pass <= 5; ++pass)
{
	Console.WriteLine("Pass " + pass.ToString() + " of 5:");
	sw.Reset();
	for (var i = 0; i < 100000; ++i)
	{
		sw.Start();
		hashset.Contains(sa[i%sa.Length]);
		sw.Stop();
	}
	Console.WriteLine("Hashtable lookups: " + sw.ElapsedMilliseconds.ToString() + "ms");
	sw.Reset();
	for (var i = 0; i < 100000; ++i)
	{
		sw.Start();
		Test.IsKeyword(sa[i % sa.Length]);
		sw.Stop();
	}
	Console.WriteLine("Fixed string lookups: " + sw.ElapsedMilliseconds.ToString() + "ms");
}
return;

