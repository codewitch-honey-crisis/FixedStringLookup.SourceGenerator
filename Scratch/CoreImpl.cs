using System.Globalization;

partial class Test
{
	[FixedStringLookup(new string[] {"fubar","foobar","foo","bar","baz"})]
	public static partial bool IsFoo(string @string);

	[FixedStringLookup(new string[] { "abstract", "as", "ascending", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "descending", "do", "double", "dynamic", "else", "enum", "equals", "explicit", "extern", "event", "false", "finally", "fixed", "float", "for", "foreach", "get", "global", "goto", "if", "implicit", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "set", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void", "volatile", "while", "yield" })]
	public static partial bool IsKeyword(string @string);

}
internal static class FixedStringLookup2
{
	internal static bool Contains(string[][] lookupTable, string value, bool ignoreCase)
	{
		int length = value.Length;
		if (length <= 0 || length - 1 >= lookupTable.Length)
			return false;
		string[] array = lookupTable[length - 1];
		return array != null && FixedStringLookup2.Contains(array, value, ignoreCase);
	}

	private static bool Contains(string[] array, string value, bool ignoreCase)
	{
		int min = 0;
		int length = array.Length;
		int num = 0;
		while (num < value.Length)
		{
			char ch = !ignoreCase ? value[num] : char.ToLower(value[num], CultureInfo.InvariantCulture);
			if (length - min <= 1)
			{
				if ((int)ch != (int)array[min][num])
					return false;
				++num;
			}
			else
			{
				if (!FixedStringLookup2.FindCharacter(array, ch, num, ref min, ref length))
					return false;
				++num;
			}
		}
		return true;
	}

	private static bool FindCharacter(
	  string[] array,
	  char value,
	  int pos,
	  ref int min,
	  ref int max)
	{
		int num1 = min;
		while (min < max)
		{
			int index1 = (min + max) / 2;
			char ch = array[index1][pos];
			if ((int)value == (int)ch)
			{
				int num2 = index1;
				while (num2 > min && (int)array[num2 - 1][pos] == (int)value)
					--num2;
				min = num2;
				int index2 = index1 + 1;
				while (index2 < max && (int)array[index2][pos] == (int)value)
					++index2;
				max = index2;
				return true;
			}
			if ((int)value < (int)ch)
				max = index1;
			else
				min = index1 + 1;
		}
		return false;
	}
}