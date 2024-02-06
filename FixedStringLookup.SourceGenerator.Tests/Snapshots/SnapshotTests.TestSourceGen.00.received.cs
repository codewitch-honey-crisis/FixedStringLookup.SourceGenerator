//HintName: FixedStringLookup.CoreImpl.g.cs
#nullable disable

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal class FixedStringLookupAttribute : System.Attribute
{
	public string[] Strings { get; set; } = null;
	public FixedStringLookupAttribute(string[] strings)
	{
		if(strings == null) throw new ArgumentNullException(nameof(strings));
		if(strings.Length == 0) throw new ArgumentException("The strings array cannot be empty",nameof(strings));
		Strings = strings;
	}
}
namespace FixedStringLookup.SourceGenerator
{
	internal static class Lookup
	{
		internal static bool Contains(string[][] lookupTable, string value)
		{
			int length = value.Length;
			if (length <= 0 || length - 1 >= lookupTable.Length)
				return false;
			string[] array = lookupTable[length - 1];
			return array != null && _Contains(array, value);
		}

		private static bool _Contains(string[] array, string value)
		{
			int min = 0;
			int length = array.Length;
			int num = 0;
			while (num < value.Length)
			{
				char ch = value[num];
				if (length - min <= 1)
				{
					if ((int)ch != (int)array[min][num])
						return false;
					++num;
				}
				else
				{
					if (!_FindCharacter(array, ch, num, ref min, ref length))
						return false;
					++num;
				}
			}
			return true;
		}

		private static bool _FindCharacter(
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
}
