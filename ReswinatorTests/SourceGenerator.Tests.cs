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
            {
            }
        }

        private static readonly string WinSDKResourceMockFileName = "WinSDKResourceMock.cs.txt";
        private static readonly string DefaultNamespace = "Sample";
        private static readonly (string, string) DefaultBuildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
        {
            { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample"}
        });

        private static IEnumerable<(string Name, SourceText Contents)> GetReswContents(IEnumerable<string> reswFiles)
        {
            return reswFiles.Select((rf) =>
            {
                var contents = VerifyGeneratorHelper.LoadSourceFromFile(rf);
                return ($"Strings/en-us/{rf}", SourceText.From(contents, Encoding.UTF8));
            });
        }

        private static IList<(string, string)> GetGeneratedOutputForFiles(IEnumerable<(string File, SourceText Contents)> inputFiles, NullableState nullableState, string targetNamespace, string language = "en-us")
        {
            var wrapperGenerator = new WrapperGenerator(targetNamespace, nullableState);
            var outputs = new List<(string, string)>();

            foreach (var inputFile in inputFiles)
            {
                var baseName = SourceGenerator.GetResourceNameFromFilename(inputFile.File, language);
                outputs.Add((baseName, wrapperGenerator.GenerateWrapperForResw(inputFile.Contents.ToString(), baseName)));
            }

            return outputs;
        }

        [Fact]
        public async void NoResourceFilesCompiles()
        {
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               new List<(string, string)>())
            {
                TestState = { AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanVerifyBasicCompilation()
        {
            var reswFiles = new[] { ("Strings/en-us/Resources.resw", SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanVerifyNonDefaultNameBasicCompilation()
        {
            var reswFiles = GetReswContents(new[] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanVerifyBasicCompilationNullableEnabled()
        {
            var reswFiles = new[] { ("Strings/en-us/Resources.resw", SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_nullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Enabled, DefaultNamespace))
            {
                NullableOption = NullableContextOptions.Enable,
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanVerifyNonDefaultNameBasicCompilationNullableEnabled()
        {
            var reswFiles = GetReswContents(new[] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_nullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Enabled, DefaultNamespace))
            {
                NullableOption = NullableContextOptions.Enable,
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact]
        public async void VerifyMultipleResourcesSingleFileNotNullable()
        {
            var reswFiles = GetReswContents(new[] { "MultipleResources.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            {
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact]
        public async void VerifyMultipleResourcesSingleFileNullable()
        {
            var reswFiles = GetReswContents(new[] { "MultipleResources.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_nullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Enabled, DefaultNamespace))
            {
                NullableOption = NullableContextOptions.Enable,
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact(Skip = "Intermittent due to test library concurrency issue")]
        public async void VerifyMultipleResourcesMultipleFilesNotNullable()
        {
            var reswFiles = GetReswContents(new[] { "MultipleResources.resw", "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            {
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact(Skip = "Intermittent due to test library concurrency issue")]
        public async void VerifyMultipleResourcesMultipleFilesNullable()
        {
            var reswFiles = GetReswContents(new[] { "MultipleResources.resw", "SingleResource.resw" });

            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_nullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Enabled, "Sample"))
            {
                NullableOption = NullableContextOptions.Enable,
                TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } }
            };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanReferenceKnownResourcesDefaultName()
        {
            var reswFiles = new[] { ("Strings/en-us/Resources.resw", SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("ConsumeResource.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void CanReferenceKnownResourcesNonDefaultName()
        {
            var reswFiles = GetReswContents(new[] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("ConsumeSingleResource.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { DefaultBuildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void NoAvailableNamespaceUsesDefaultName()
        {
            var reswFiles = GetReswContents(new[] { "SingleResource.resw" });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, "GeneratedResources"))
            { TestState = { AdditionalFiles = { reswFiles } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void NamespaceFromBuildConfigReflectedCorrectly()
        {
            var TARGET_NAMESPACE = "MagicNamespace";
            var reswFiles = GetReswContents(new[] { "SingleResource.resw" });
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new() { { SourceGenerator.NAMESPACE_BUILD_PROPERTY, TARGET_NAMESPACE } });
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, TARGET_NAMESPACE))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }

        [Theory]
        [InlineData("Strings/en-us/Resources.resw", "en-us")]
        [InlineData("Strings/fr-fr/Resources.resw", "fr-fr")]
        [InlineData("Strings/language-en-us/Resources.resw", "en-us")]
        [InlineData("Strings/Resources.en-us.resw", "en-us")]
        [InlineData("Strings/en-us/Wibble/Resources.resw", "en-us")]
        [InlineData("Strings/Qux/en-us/Wibble/Resources.resw", "en-us")]
        [InlineData("Strings/foo/bar/baz/Resources.language-en-us.resw", "en-us")]
        [InlineData("Strings/foo/bar/baz/Other.Resources.language-en-us.resw", "en-us")]
        [InlineData("Strings/qux/Res.our.ces.en-us.resw", "en-us")]
        public async void VerifyLanguageResolution(string path, string language)
        {
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
            {
                { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample" },
                { SourceGenerator.DEFAULT_LANGUAGE_BUILD_PROPERTY, language }
            });

            var reswFiles = new[] { (path, SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace, language))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }

        [Theory]
        [InlineData("Strings/en-us/Resources.resw", "en-us")]
        [InlineData("Strings/fr-fr/Resources.resw", "fr-fr")]
        [InlineData("Strings/language-en-us/Resources.resw", "en-us")]
        [InlineData("Strings/Resources.en-us.resw", "en-us")]
        [InlineData("Strings/en-us/Wibble/Resources.resw", "en-us")]
        [InlineData("Strings/Qux/en-us/Wibble/Resources.resw", "en-us")]
        [InlineData("Strings/foo/bar/baz/Resources.language-en-us.resw", "en-us")]
        [InlineData("C:/Not/Rooted/Strings/foo/en-us/Something/Resources.resw", "en-us")]
        public async void VerifyLanguageResolutionWithRoot(string path, string language)
        {
            var ROOT = "C:/Something/Is/Happening";
            if (!path.StartsWith("C:/"))
            {
                path = $"{ROOT}/{path}";
            }
            
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
            {
                { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample" },
                { SourceGenerator.DEFAULT_LANGUAGE_BUILD_PROPERTY, language },
                { SourceGenerator.PROJECT_DIRECTORY_BUILD_PROPERTY, ROOT }
            });

            var reswFiles = new[] { (path, SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace, language))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void NoSourceGeneratedWhenReswInNonCompliantPath()
        {
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
            {
                { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample" },
                { SourceGenerator.DEFAULT_LANGUAGE_BUILD_PROPERTY, "en-us" }
            });

            var reswFiles = new[] { ("Strings/Foo/Resources.resw", SourceText.From(VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw"), Encoding.UTF8)) };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               new (string, string) [] { })
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }

        [Fact(Skip = "Multiple files, inconsistent results")]
        public async void MultipleResourceFilesThatResolveToSameClassName()
        {
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
            {
                { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample" },
                { SourceGenerator.DEFAULT_LANGUAGE_BUILD_PROPERTY, "en-us" }
            });

            var reswContent = VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw");

            var reswFiles = new[]
            {
                ("Strings/en-us/Resources.resw", SourceText.From(reswContent, Encoding.UTF8)),
                ("Strings/en-us/Res.our.ces.resw", SourceText.From(reswContent, Encoding.UTF8))
            };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(reswFiles, NullableState.Disabled, DefaultNamespace, "en-us"))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }

        [Fact]
        public async void MultipleLanguagesSameResourceNameShouldResolveToOneClass()
        {
            var buildConfig = VerifyGeneratorHelper.GlobalConfigFor(new()
            {
                { SourceGenerator.NAMESPACE_BUILD_PROPERTY, "Sample" },
                { SourceGenerator.DEFAULT_LANGUAGE_BUILD_PROPERTY, "en-us" }
            });

            var reswContent = VerifyGeneratorHelper.LoadSourceFromFile("SingleResource.resw");

            var reswFiles = new[]
            {
                ("Strings/en-us/Resources.resw", SourceText.From(reswContent, Encoding.UTF8)),
                ("Strings/fr-fr/Resources.resw", SourceText.From(reswContent, Encoding.UTF8))
            };
            var verifier = new ReswinatorVerifyHelper("SimpleSourceFile_notnullable.cs.txt",
                                               GetGeneratedOutputForFiles(new []{ reswFiles.First() }, NullableState.Disabled, DefaultNamespace, "en-us"))
            { TestState = { AdditionalFiles = { reswFiles }, AnalyzerConfigFiles = { buildConfig } } };

            await verifier.RunAsync();
        }
    }
}