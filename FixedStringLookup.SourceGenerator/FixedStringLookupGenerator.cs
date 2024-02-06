using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace FixedStringLookup.SourceGenerator
{
	[Generator]
	public class FixedStringLookupGenerator : IIncrementalGenerator
	{
		const string _NullableOff = @"#nullable disable
";
		const string _CoreImpl = @"#nullable disable

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal class FixedStringLookupAttribute : System.Attribute
{
	public string[] Strings { get; set; } = null;
	public FixedStringLookupAttribute(string[] strings)
	{
		if(strings == null) throw new ArgumentNullException(nameof(strings));
		if(strings.Length == 0) throw new ArgumentException(""The strings array cannot be empty"",nameof(strings));
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
			if (length <= 0 || length >= lookupTable.Length)
				return false;
			string[] array = lookupTable[length];
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
";
		static string _GetNamespace(BaseTypeDeclarationSyntax syntax)
		{
			// If we don't have a namespace at all we'll return an empty string
			// This accounts for the "default namespace" case
			string nameSpace = string.Empty;

			// Get the containing syntax node for the type declaration
			// (could be a nested type, for example)
			SyntaxNode potentialNamespaceParent = syntax.Parent;

			// Keep moving "out" of nested classes etc until we get to a namespace
			// or until we run out of parents
			while (potentialNamespaceParent != null &&
					potentialNamespaceParent is not NamespaceDeclarationSyntax
					&& potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
			{
				potentialNamespaceParent = potentialNamespaceParent.Parent;
			}

			// Build up the final namespace by looping until we no longer have a namespace declaration
			if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
			{
				// We have a namespace. Use that as the type
				nameSpace = namespaceParent.Name.ToString();

				// Keep moving "out" of the namespace declarations until we 
				// run out of nested namespace declarations
				while (true)
				{
					if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
					{
						break;
					}

					// Add the outer namespace as a prefix to the final namespace
					nameSpace = $"{namespaceParent.Name}.{nameSpace}";
					namespaceParent = parent;
				}
			}

			// return the final namespace
			return nameSpace;
		}
		static void _AppendLiteral(string input,StringBuilder sb)
		{
			sb.Append("\"");
			foreach (var c in input)
			{
				switch (c)
				{
					case '\"': sb.Append("\\\""); break;
					case '\\': sb.Append(@"\\"); break;
					case '\0': sb.Append(@"\0"); break;
					case '\a': sb.Append(@"\a"); break;
					case '\b': sb.Append(@"\b"); break;
					case '\f': sb.Append(@"\f"); break;
					case '\n': sb.Append(@"\n"); break;
					case '\r': sb.Append(@"\r"); break;
					case '\t': sb.Append(@"\t"); break;
					case '\v': sb.Append(@"\v"); break;
					default:
						// ASCII printable character
						if (c >= 0x20 && c <= 0x7e)
						{
							sb.Append(c);
							// As UTF16 escaped character
						}
						else
						{
							sb.Append(@"\u");
							sb.Append(((int)c).ToString("x4"));
						}
						break;
				}
			}
			sb.Append("\"");
		}
		static void _RenderLookupTable(string tab, string[] strings, StringBuilder sb)
		{
			Array.Sort<string>(strings, (x, y) => { int cmp = x.Length - y.Length; if (cmp == 0) return string.CompareOrdinal(x, y); return cmp; });

			if (strings.Length == 0)
			{
				sb.AppendLine("new string[0];");
				return;
			}
			var count = strings[strings.Length - 1].Length;
			sb.AppendLine("new string[" + count + "][] {");
			var si = 0;
			var strs = new List<string>(strings.Length);
			for(int i = 0;i<count;++i)
			{
				strs.Clear();
				var first = true;
				while (si<strings.Length && strings[si].Length==i)
				{
					if(first)
					{
						sb.Append(tab + "    new string[] { ");
						first = false;
					} else
					{
						sb.Append(", ");
					}
					_AppendLiteral(strings[si++], sb);
				}
				if(first)
				{
					sb.Append(tab+"    null");
				} else
				{
					sb.Append(" }");
				}
				if(i<count-1)
				{
					sb.Append(',');
				}
				sb.AppendLine();
				
			}
			sb.AppendLine(tab+"};");

		}
		static void _Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
		{
			if(methods.IsEmpty)
			{
				return; // nothing to do
			}
			methods.Sort((x, y) =>
			{
				var xp = x.Parent as TypeDeclarationSyntax; var yp = y.Parent as TypeDeclarationSyntax;
				var cmp = xp.Identifier.ToFullString().CompareTo(yp.Identifier.ToFullString());
				if (cmp == 0) return x.Identifier.ToFullString().CompareTo(y.Identifier.ToFullString());
				return cmp;
			});
			var sb = new StringBuilder();
			string oldNS = null;
			var trailingBrace = false;
			var clstab = "";
			var mtab = "    ";
			string oldType = null;
			string type = null;
			for (int i = 0;i < methods.Length;i++)
			{
				var method = methods[i];
				SemanticModel semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
				ISymbol methodSymbol = semanticModel.GetDeclaredSymbol(method);
				var parent = method.Parent as TypeDeclarationSyntax;
				var ns = _GetNamespace(parent);
				if (ns != oldNS)
				{
					if (!string.IsNullOrEmpty(oldNS))
					{
						sb.AppendLine("}");
						trailingBrace = false;
					}
					if (ns.Length > 0)
					{
						sb.AppendLine("namespace " + ns);
						sb.AppendLine("{");
						clstab = "    ";
						mtab = clstab + clstab;
						trailingBrace = true;
					}
					oldNS = ns;
				}
				type = parent.Identifier.ToFullString().Trim();
				if (oldType != type)
				{
					if (oldType != null)
					{
						sb.AppendLine(clstab + "}");
						sb.AppendLine();
					}
					oldType = type;
					sb.AppendLine(clstab + "partial class " + parent.Identifier.ToString());
					sb.AppendLine(clstab + "{");
					oldType = type;
				}
				var attrs = methodSymbol.GetAttributes();
				foreach (var attr in attrs)
				{
					if (attr.AttributeClass != null && attr.AttributeClass.ToDisplayString() == "FixedStringLookupAttribute")
					{
						ImmutableArray<TypedConstant>? sa = null;
						if (attr.ConstructorArguments != null && attr.ConstructorArguments.Length == 1)
						{
							sa = attr.ConstructorArguments[0].Values;
						}
						if (sa != null)
						{
							// render the data
							sb.Append(mtab + "private static string[][] _" + method.Identifier.ToString().Trim());
							sb.Append(" = ");
							var sarr = new string[sa.Value.Length];
							for(int k = 0;k<sa.Value.Length;k++)
							{
								sarr[k]= sa.Value[k].Value as string;
							}
							_RenderLookupTable(mtab, sarr, sb);
							// render the method
							var access = "public";
							var pub = false;
							foreach (var mod in method.Modifiers)
							{
								if (mod.Text == "public")
								{
									pub = true;
									break;
								}
							}
							var intr = false;
							foreach (var mod in method.Modifiers)
							{
								if (mod.Text == "internal")
								{
									intr = true;
									break;
								}
							}
							if (!pub)
							{
								access = intr ? "internal" : "private";
							}
							sb.Append(mtab + access + " static partial bool " + method.Identifier.ToString().Trim());
							sb.Append('(');
							sb.AppendLine("string @string) => FixedStringLookup.SourceGenerator.Lookup.Contains(_" + method.Identifier.ToString().Trim() + ", @string);");
						}
					}
				}
			}
			
			if (methods.Length > 0)
			{
				sb.AppendLine(clstab + "}");
				sb.AppendLine();
			}
			if (trailingBrace)
			{
				sb.AppendLine("}");
				sb.AppendLine();
			}
			if (sb.Length > 0)
			{
				context.AddSource("FixedStringLookup.Methods.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
			}
		}
		static MethodDeclarationSyntax _GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
		{
			var decl = (MethodDeclarationSyntax)context.Node;

			// loop through all the attributes on the method
			foreach (AttributeListSyntax attributeListSyntax in decl.AttributeLists)
			{
				foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
				{
					var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
					if (attributeSymbol==null)
					{
						// weird, we couldn't get the symbol, ignore it
						continue;
					}

					INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
					string fullName = attributeContainingTypeSymbol.ToDisplayString();

					// Is the attribute the our attribute?
					if (fullName == "FixedStringLookupAttribute")
					{
						// return the method
						return decl;
					}
				}
			}

			// we didn't find the attribute we were looking for
			return null;
		}
		static bool _IsSyntaxTargetForGeneration(SyntaxNode node)
			=> node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0;
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			// Add the marker attribute and helper function to the compilation
			context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
				"FixedStringLookup.CoreImpl.g.cs",
				SourceText.From(_CoreImpl, Encoding.UTF8)));
			IncrementalValuesProvider<MethodDeclarationSyntax> toGenerate = context.SyntaxProvider
		   .CreateSyntaxProvider(
			   predicate: static (s, _) => _IsSyntaxTargetForGeneration(s), // select enums with attributes
			   transform: static (ctx, _) => _GetSemanticTargetForGeneration(ctx)) // select enums with the [EnumExtensions] attribute and extract details
		   .Where(static m => m is not null); // Filter out errors that we don't care about
			IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods
			   = context.CompilationProvider.Combine(toGenerate.Collect());
			context.RegisterSourceOutput(compilationAndMethods,
			static (spc, source) => _Execute(source.Item1, source.Item2, spc));

		}
	}
}
