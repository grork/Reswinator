using System.Text;

namespace Codevoid.Utilities.Reswinator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public static readonly string NAMESPACE_BUILD_PROPERTY = "build_property.RootNamespace";
    public static readonly string DEFAULT_LANGUAGE_BUILD_PROPERTY = "build_property.DefaultLanguage";
    public static readonly string PROJECT_DIRECTORY_BUILD_PROPERTY = "build_property.ProjectDir";

    public void Execute(GeneratorExecutionContext context)
    {
        var defaultLanguage = context.GetBuildProperty(DEFAULT_LANGUAGE_BUILD_PROPERTY, "en-us");
        var projectRoot = context.GetBuildProperty(PROJECT_DIRECTORY_BUILD_PROPERTY, String.Empty);

        // Find any resw files that we might process
        var reswFiles = (from f in context.AdditionalFiles
                         where ShouldGenerateWrapperFor(f.Path, projectRoot, defaultLanguage)
                         select f).ToList();

        // Derive the nullable state so we can emit ?'s if needed.
        var nullableState = (context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable) ? NullableState.Enabled : NullableState.Disabled;

        // Attempt to source the target namespace from the exposed RootNamespace
        // build property. This is a *project* property that is exposed by the
        // props that includes the generator. The compilation itself has no
        // concept of default namespace, so we have to hoist it from the project
        var targetNamespace = context.GetBuildProperty(NAMESPACE_BUILD_PROPERTY, "GeneratedResources");

        // Generate per-resw
        var wrapperGenerator = new WrapperGenerator(targetNamespace, nullableState);
        foreach (var file in reswFiles)
        {
            var baseFilename = Path.GetFileNameWithoutExtension(file.Path);
            var contents = wrapperGenerator.GenerateWrapperForResw(file.GetText()!.ToString(), baseFilename);
            context.AddSource(baseFilename, contents);
        }
    }

    public void Initialize(GeneratorInitializationContext context) { }

    /// <summary>
    /// Given a file path, conclude if it's relevant to our interests (E.g. a
    /// resw file in a path that contains the default language)
    /// </summary>
    /// <param name="path">Path to inspect</param>
    /// <param name="root">Root of the path</param>
    /// <param name="language">Language we're looking for</param>
    /// <returns>True if we should generate a wrapper for this file</returns>
    private static bool ShouldGenerateWrapperFor(string path, string root, string language)
    {
        if(Path.GetExtension(path) != ".resw")
        {
            return false;
        }

        var relativePath = (String.IsNullOrEmpty(root) ? path : path.Replace(root, String.Empty));
        return relativePath.IndexOf(language, StringComparison.OrdinalIgnoreCase) > -1;
    }
}

internal static class GeneratorExecutionContextExtensions
{
    internal static string GetBuildProperty(this GeneratorExecutionContext context, string key, string defaultValue)
    {
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(key, out var storedValue))
        {
            return defaultValue;
        }

        return storedValue;
    }
}