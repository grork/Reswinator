using System;
using System.Text;
using Codevoid.Utilities.Reswinator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Codevoid.Test.Reswinator
{
    public static class ListExtensions
    {
        public static void Add(this SourceFileCollection instance, IEnumerable<(string, SourceText)> range)
        {
            instance.AddRange(range);
        }
    }

    public class SourceGeneratorTests
    {
        /// <summary>
        /// Simplifies consistent injection of the Resource mock into the test
        /// hepler.
        /// </summary>
        private class ReswinatorVerifyHelper : VerifyGeneratorHelper<SourceGenerator>
        {
            public ReswinatorVerifyHelper(string inputFile, IEnumerable<(string BaseName, string Content)> generatedSources) : base(new String[] { WinSDKResourceMockFileName, inputFile }, generatedSources)
            { }
        }

        private static readonly string WinSDKResourceMockFileName = "WinSDKResourceMock.cs.txt";
        private static IEnumerable<(string, SourceText)> GetReswContents(IEnumerable<string> reswFiles)
        {
            return reswFiles.Select((rf) =>
            {
                var contents = VerifyGeneratorHelper.LoadSourceFromFile(rf);
                return (rf, SourceText.From(contents, Encoding.UTF8));
            });
        }

        private static IList<(string, string)> GetGeneratedOutputForFiles(IEnumerable<(string File, SourceText Contents)> inputFiles, NullableState nullableState, string targetNamespace)
        {
            var wrapperGenerator = new WrapperGenerator(targetNamespace, nullableState);
            var outputs = new List<(string, string)>();

            foreach (var inputFile in inputFiles)
            {
                var baseName = Path.GetFileNameWithoutExtension(inputFile.File);
                outputs.Add((baseName, wrapperGenerator.GenerateWrapperForResw(inputFile.Contents.ToString(), baseName)));
            }

            return outputs;
        }

        [Fact]
        public async void CanVerifyBasicCompilation()
        {
            var reswFiles = GetReswContents(new [] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, "Sample"))
            { TestState = { AdditionalFiles = { reswFiles } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanVerifyBasicCompilationNullableEnabled()
        {
            var reswFiles = GetReswContents(new [] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_nullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Enabled, "Sample"))
            {
                NullableOption = NullableContextOptions.Enable,
                TestState = { AdditionalFiles = { reswFiles } }
            };

            await verifier.RunAsync();
        }
    }
}

