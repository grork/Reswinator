using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Codevoid.Test.Reswinator;

/// <summary>
/// Helps test Source Generators by wrapping all the work of setting up the
/// Analyzers state &amp; input correctly for automatic validation.
/// 
/// Originally perloined from https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
/// </summary>
/// <typeparam name="TSourceGenerator">Generator type to be instantiated</typeparam>
internal class VerifyGeneratorHelper<TSourceGenerator> : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
                               where TSourceGenerator : ISourceGenerator, new()
{
    public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

    /// <summary>
    /// Target nullable configuration. Defaults to disabled.
    /// </summary>
    public NullableContextOptions NullableOption = NullableContextOptions.Disable;

    /// <summary>
    /// Construct with the input file to compile, and the expected generated
    /// filenames <i>and</i> content.
    /// </summary>
    /// <param name="inputFilePath">Souce file to compile</param>
    /// <param name="generatedSources">Generated files + content</param>
    public VerifyGeneratorHelper(string inputFilePath, IList<(string BaseName, string Content)> generatedSources)
    {
        this.Initialize(inputFilePath, generatedSources);
    }

    public VerifyGeneratorHelper(string inputFilePath, IList<string> generatedSourceNames)
    {
        // Convert the files into files + content by loading the content from the
        // samples folder.
        var withContent = generatedSourceNames.Select<string, (string, string)>((s) =>
        {
            var content = VerifyGeneratorHelper.LoadSourceFromFile($"{s}.cs.txt");
            return (s, content);
        }).ToList();

        this.Initialize(inputFilePath, withContent);
    }

    private void Initialize(string inputFilePath, IList<(string BaseName, string Content)> generatedSources)
    {
        this.TestState.Sources.Add(VerifyGeneratorHelper.LoadSourceFromFile(inputFilePath));

        foreach (var generatedSourceName in generatedSources)
        {
            this.TestState.GeneratedSources.Add((typeof(TSourceGenerator), $"{generatedSourceName.BaseName}.cs", generatedSourceName.Content));
        }
    }

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable", "/nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }

    protected override CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = (CSharpCompilationOptions)base.CreateCompilationOptions();
        if (this.NullableOption != NullableContextOptions.Disable)
        {
            compilationOptions = compilationOptions.WithNullableContextOptions(this.NullableOption);
        }

        return compilationOptions.WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    }

    protected override ParseOptions CreateParseOptions()
    {
        return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
    }
}

internal static class VerifyGeneratorHelper
{
    /// <summary>
    /// Generates a global configuration file (.editconfig-style) to allow global
    /// variables from the project system to be unit tested in the source
    /// generator
    /// </summary>
    /// <param name="values">Name value pairs to include</param>
    /// <returns>Tuples to be used in AnalyzerConfigFiles</returns>
    internal static (string Name, string Contents) GlobalConfigFor(Dictionary<string, string> values)
    {
        StringBuilder config = new StringBuilder("is_global = true");
        config.AppendLine();

        foreach (var kvp in values)
        {
            config.Append($"{kvp.Key} = {kvp.Value}");
            config.AppendLine();
        }

        return ("/.globalconfig", config.ToString());
    }

    internal static string LoadSourceFromFile(string path)
    {
        return File.ReadAllText(Path.Join("samples", path), Encoding.UTF8);
    }
}