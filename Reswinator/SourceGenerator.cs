namespace Codevoid.Utilities.Reswinator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Find any resw files that we might process
        var reswFiles = (from f in context.AdditionalFiles
                        where Path.GetExtension(f.Path).ToLowerInvariant() == ".resw"
                        select f).ToList();

        // Derive the nullable state so we can emit ?'s if needed.
        var nullableState = (context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable) ? NullableState.Enabled : NullableState.Disabled;

        // Generate per-resw
        var wrapperGenerator = new WrapperGenerator("Sample", nullableState);
        foreach (var file in reswFiles)
        {
            var baseFilename = Path.GetFileNameWithoutExtension(file.Path);
            var contents = wrapperGenerator.GenerateWrapperForResw(file.GetText()!.ToString(), baseFilename);
            context.AddSource(baseFilename, contents);
        }
    }

    public void Initialize(GeneratorInitializationContext context) { }
}