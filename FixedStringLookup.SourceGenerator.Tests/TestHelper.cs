using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new List<PortableExecutableReference>()  
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };
        
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        var generator = new FixedStringLookup.SourceGenerator.FixedStringLookupGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier
            .Verify(driver)
            .UseDirectory("Snapshots");
    }
}