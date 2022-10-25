
using System.Xml;

namespace Codevoid.Utilities.Reswinator;

internal enum NullableState
{
    Enabled,
    Disabled
}

/// <summary>
/// Generates source code to access resources in a supplied ResW content
/// </summary>
internal class WrapperGenerator
{
    /// <summary>
    /// Data class to bundle related resource data together
    /// </summary>
    /// <param name="Resources">The property + resource names in this container</param>
    /// <param name="Children">Any nested resources</param>
    private record ResourceContainer(IDictionary<string, string> Resources, IDictionary<string, ResourceContainer> Children);

    // "Constants" that are emited mulitple times
    private static readonly string FQ_RESOURCE_LOADER = "global::Microsoft.Windows.ApplicationModel.Resources.ResourceLoader";
    private static readonly string EDITOR_BROWSER_ATTRIBUTE = "[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]";

    private readonly OutputWriter writer = new OutputWriter();
    private readonly string fullyQualifiedTargetNamespace;
    private readonly string accessModifier = "internal";
    private readonly string selfFullyQualifiedTypeName = typeof(WrapperGenerator).FullName;
    private readonly string selfVersion = typeof(WrapperGenerator).Assembly.GetName().Version.ToString();
    private readonly string nullableSuffix;

    internal WrapperGenerator(string targetNamespace, NullableState nullableState = NullableState.Enabled, string? version = null)
    {
        this.fullyQualifiedTargetNamespace = targetNamespace;

        // Allow the version to be overriden for testing (So that the value is
        // stable over time, and doesn't change the test-validation hash)
        if (!String.IsNullOrEmpty(version))
        {
            selfVersion = version!;
        }

        this.nullableSuffix = (nullableState == NullableState.Enabled) ? "?" : "";
    }

    /// <summary>
    /// For the supplied ResW contents, generates an accessor class that
    /// provides static, strongly typed access to the resources in that ResW
    /// </summary>
    /// <param name="reswContents">The contents of the ResW have to be wrapped</param>
    /// <param name="resourceMapName">
    /// The assumed name of the resource map that will be be read from at
    /// runtime
    /// </param>
    /// <returns>Source code for the wrapper</returns>
    internal string GenerateWrapperForResw(string reswContents, string resourceMapName = "Resources")
    {
        var reswXml = new XmlDocument();
        reswXml.LoadXml(reswContents);

        if (reswXml.DocumentElement == null)
        {
            return String.Empty;
        }

        var dataElements = reswXml.SelectNodes("//data");
        if (dataElements is null || dataElements.Count == 0)
        {
            return string.Empty;
        }

        // Load the resources from the ResW file. Parse out 'nested' resources
        // (of the form Foo.Bar.Baz) into a nested structure so properties are
        // generated into the containing class
        var parsedResources = new ResourceContainer(new Dictionary<string, string>(), new Dictionary<string, ResourceContainer>());
        foreach (XmlNode n in dataElements)
        {
            XmlElement? element = n as XmlElement;
            if (element is null || !element.HasAttribute("name"))
            {
                continue;
            }

            var resourceName = element.GetAttribute("name");
            var segments = resourceName.Split('.');
            if (segments.Length > 1)
            {
                WrapperGenerator.ProcessNestedResource(segments, parsedResources);
                continue;
            }

            // Since this is a top level item, the property name and the actual
            // resource name will be the same (E.g. they aren't .'d)
            parsedResources.Resources.Add(resourceName, resourceName);
        }

        this.StartNamespace();
        this.writer.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{this.selfFullyQualifiedTypeName}\", \"{this.selfVersion}\")]");
        this.WriteResourceContainer(parsedResources, resourceMapName, resourceMapName);
        this.EndNamespace();

        return this.writer.GetOutput();
    }

    /// <summary>
    /// Processes a resource that is 'nested' (E.g., has . in it's name), adding
    /// it to the appropriate nested list for co-located output
    /// </summary>
    /// <param name="nameParts">The resource name separated by '.'</param>
    /// <param name="rootContainer">Root container to search search from</param>
    private static void ProcessNestedResource(string[] nameParts, ResourceContainer rootContainer)
    {
        var propertyName = nameParts.Last(); // Actual resource name
        var segments = nameParts.Take(nameParts.Length - 1); // prefix path

        foreach (var segment in segments)
        {
            // Create a new container if we don't already have one
            rootContainer.Children.TryGetValue(segment, out var childContainer);
            if (childContainer is null)
            {
                childContainer = new ResourceContainer(new Dictionary<string, string>(), new Dictionary<string, ResourceContainer>());
                rootContainer.Children[segment] = childContainer;
            }

            rootContainer = childContainer;
        }

        rootContainer.Resources.Add(propertyName, String.Join("/", nameParts));
    }

    /// <summary>
    /// Writes the supplied container into the output, with properties for each
    /// contained resource
    /// </summary>
    /// <param name="resourceContainer">Container to emit</param>
    /// <param name="className">Class name for this container</param>
    /// <param name="resourceMapName">
    /// Optional resource map name, if the name is required (E.g. not nested)
    /// </param>
    private void WriteResourceContainer(ResourceContainer resourceContainer, string className, string resourceMapName = "")
    {
        this.StartClass(className);

        // We only need to write the lazy-init property if we're accessing the
        // resource map directly. If we're contained in a larger context, we can
        // just use the already-defined-in-scope one.
        if (!String.IsNullOrEmpty(resourceMapName))
        {
            this.WriteResourceLoaderLazyProperty(resourceMapName);
        }

        foreach (var resource in resourceContainer.Resources)
        {
            var propertyName = resource.Key;

            // When there is:
            // Foo.Bar
            // Foo.Bar.Baz
            // 'Baz will cause class to be generated, conflicting with the
            // property for `Bar. This will caue a compile failure. While docs
            // say this isn't a supported path, I don't want to emit code that
            // can't be compiled.
            //
            // So, suffix the property name with _Resource
            if (resourceContainer.Children.ContainsKey(resource.Key))
            {
                propertyName = $"{propertyName}_Resource";
            }

            this.WritePropertyAccessorForResource(propertyName, resource.Value);
        }

        // Write any nested children
        if (resourceContainer.Children.Count > 0)
        {
            this.writer.NewLine();

            foreach (var kvp in resourceContainer.Children)
            {
                this.WriteResourceContainer(kvp.Value, kvp.Key);
            }
        }

        this.EndClass();
    }

    private void StartNamespace()
    {
        this.writer.WriteLine($"namespace {this.fullyQualifiedTargetNamespace} {{");
        this.writer.Indent();
    }

    private void StartClass(string targetClassName)
    {
        // Attributes to assist the debugger
        this.writer.WriteLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
        this.writer.WriteLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
        this.writer.WriteLine($"{this.accessModifier} static class {targetClassName} {{");
        this.writer.Indent();
    }

    private void WriteResourceLoaderLazyProperty(string resourceMapName)
    {
        // Resource Loader lazy field
        this.writer.WriteLine($"private static {FQ_RESOURCE_LOADER}{this.nullableSuffix} resourceLoader;");
        this.writer.NewLine();

        // Resource Loader property declaration
        this.writer.WriteLine(EDITOR_BROWSER_ATTRIBUTE);
        this.writer.WriteLine($"private static {FQ_RESOURCE_LOADER} Loader {{");
        this.writer.Indent();

        // Resource Loader property getter
        this.writer.WriteLine("get {");
        this.writer.Indent();

        // Resource Loader null check, instantiation, and return
        this.writer.WriteLine("if (resourceLoader is null) {");
        this.writer.Indent();

        var isDefaultResourceMap = String.Equals(resourceMapName, "Resources", StringComparison.InvariantCultureIgnoreCase);
        this.writer.WriteLine($"resourceLoader = new {FQ_RESOURCE_LOADER}({(isDefaultResourceMap ? "" : $"\"{resourceMapName}\"")});");

        this.writer.Dindent();
        this.writer.WriteLine("}");

        this.writer.WriteLine("return resourceLoader;");
        this.writer.Dindent();
        this.writer.WriteLine("}");

        // End Property
        this.writer.Dindent();
        this.writer.WriteLine("}");
        this.writer.NewLine();
    }

    private void WritePropertyAccessorForResource(string propertyName, string resourceName)
    {
        this.writer.WriteLine($"{accessModifier} static string {propertyName} => Loader.GetString(\"{resourceName}\");");
    }

    private void EndClass() => this.EndNamespace();

    private void EndNamespace()
    {
        this.writer.Dindent();
        this.writer.WriteLine("}");
    }
}