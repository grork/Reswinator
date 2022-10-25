using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Codevoid.Test.Reswinator;

internal class SampleGenerator : ISourceGenerator
{
    internal static readonly string SAMPLE_FILENAME = "Sample.Generated";
    internal static readonly string SAMPLE_OUTPUT = "// Sample Generator";
    internal static readonly string BUILD_PROPERTY_NAME = "build_properties.SampleGenerator_Fail";

    public void Execute(GeneratorExecutionContext context)
    {
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(BUILD_PROPERTY_NAME, out var propertyValue);
        if (propertyValue is "")
        {
            var descriptor = new DiagnosticDescriptor(id: "SG1001", "No Value", "Missing Value in {0}", typeof(SampleGenerator).Name, DiagnosticSeverity.Error, true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, BUILD_PROPERTY_NAME));
        }

        if (propertyValue == "fail")
        {
            var descriptor = new DiagnosticDescriptor(id: "SG1002", "Explicit Fail", "Explicit Fail {0}", typeof(SampleGenerator).Name, DiagnosticSeverity.Error, true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, BUILD_PROPERTY_NAME));
        }
        context.AddSource(SAMPLE_FILENAME, SAMPLE_OUTPUT);

    }

    public void Initialize(GeneratorInitializationContext context) { }
}

public class SampleGeneratorTests
{
    [Fact]
    public async void CanVerifyBasicCompilationWithSampleGenerator()
    {

        var f = new VerifyGeneratorHelper<SampleGenerator>("SimpleSourceFile_notnullable.cs.txt",
                                                           new List<(string, string)>() { (SampleGenerator.SAMPLE_FILENAME, SampleGenerator.SAMPLE_OUTPUT) });
        await f.RunAsync();
    }

    [Fact]
    public async void CanVerifyBasicCompilationWithSampleGeneratorWithNullablesEnabledReportingError()
    {
        var f = new VerifyGeneratorHelper<SampleGenerator>("SimpleSourceFile_nullable.cs.txt",
                                                           new List<(string, string)>() { (SampleGenerator.SAMPLE_FILENAME, SampleGenerator.SAMPLE_OUTPUT) })
        {
            ExpectedDiagnostics =
            {
                DiagnosticResult.CompilerError("CS8632").WithSpan(3, 19, 3, 20)
            }
        };
        await f.RunAsync();
    }

    [Fact]
    public async void GlobalBuildVariableAvailableToGenerator()
    {
        var f = new VerifyGeneratorHelper<SampleGenerator>("SimpleSourceFile_notnullable.cs.txt",
                                                           new List<(string, string)>() { (SampleGenerator.SAMPLE_FILENAME, SampleGenerator.SAMPLE_OUTPUT) })
        {
            TestState = { AnalyzerConfigFiles = { VerifyGeneratorHelper.GlobalConfigFor(new() { { SampleGenerator.BUILD_PROPERTY_NAME, "fail" } }) } },
            ExpectedDiagnostics = { DiagnosticResult.CompilerError("SG1002").WithArguments("build_properties.SampleGenerator_Fail") }
        };
        await f.RunAsync();
    }

    [Fact]
    public async void GlobalBuildVariableAvailableToGeneratorWithEmptyValue()
    {
        var f = new VerifyGeneratorHelper<SampleGenerator>("SimpleSourceFile_notnullable.cs.txt",
                                                           new List<(string, string)>() { (SampleGenerator.SAMPLE_FILENAME, SampleGenerator.SAMPLE_OUTPUT) })
        {
            TestState = { AnalyzerConfigFiles = { VerifyGeneratorHelper.GlobalConfigFor(new() { { SampleGenerator.BUILD_PROPERTY_NAME, "" } }) } },
            ExpectedDiagnostics = { DiagnosticResult.CompilerError("SG1001").WithArguments("build_properties.SampleGenerator_Fail") }
        };

        await f.RunAsync();
    }
}

