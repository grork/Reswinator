namespace Codevoid.Utilities.Reswinator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public static readonly string NAMESPACE_BUILD_PROPERTY = "build_properties.RootNamespace";
    
    public void Execute(GeneratorExecutionContext context)
    {
        // Find any resw files that we might process
        var reswFiles = (from f in context.AdditionalFiles
                        where Path.GetExtension(f.Path).ToLowerInvariant() == ".resw"
                        select f).ToList();

        // Derive the nullable state so we can emit ?'s if needed.
        var nullableState = (context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable) ? NullableState.Enabled : NullableState.Disabled;
        
        // Attempt to source the target namespace from the exposed RootNamespace
        // build property. This is a *project* property that is exposed by the
        // props that includes the generator. The compilation itself has no
        // concept of default namespace, so we have to hoist it from the project
        var targetNamespace = "GeneratedResources"; // Default incase we can't get the project property
        if(context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(NAMESPACE_BUILD_PROPERTY, out var namespacePropertyValue))
        {
            targetNamespace = namespacePropertyValue;
        }

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
}