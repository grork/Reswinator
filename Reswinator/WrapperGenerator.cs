
using System.Xml;

namespace Codevoid.Utilities.Reswinator;

/// <summary>
/// Generates source code to access resources in a supplied ResW content
/// </summary>
internal class WrapperGenerator
{
    private static readonly string FQ_RESOURCE_LOADER = "global::Microsoft.Windows.ApplicationModel.Resources.ResourceLoader";
    private static readonly string EDITOR_BROWSER_ATTRIBUTE = "[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]";
    private readonly OutputWriter writer = new OutputWriter();
    private readonly string fullyQualifiedTargetNamespace;
    private readonly string accessModifier = "internal";
    private readonly string selfFullyQualifiedTypeName = typeof(WrapperGenerator).FullName;
    private readonly string selfVersion;

    internal WrapperGenerator(string targetNamespace, string? version = null)
    {
        this.fullyQualifiedTargetNamespace = targetNamespace;
        if(String.IsNullOrEmpty(version))
        {
            version = typeof(WrapperGenerator).Assembly.GetName().Version.ToString();
        }

        selfVersion = version!;
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

        if(reswXml.DocumentElement == null)
        {
            return String.Empty;
        }

        var dataElements = reswXml.SelectNodes("//data");
        if(dataElements is null || dataElements.Count == 0)
        {
            return string.Empty;
        }

        this.StartNamespace();
        this.StartClass(resourceMapName);

        this.writer.NewLine();

        foreach (XmlNode n in dataElements)
        {
            XmlElement? element = n as XmlElement;
            if(element is null || !element.HasAttribute("name"))
            {
                continue;
            }

            var resourceName = element.GetAttribute("name");

            this.WritePropertyAccessorForResource(resourceName);
        }

        this.EndClass();
        this.EndNamespace();

        return this.writer.GetOutput();
    }

    private void StartNamespace()
    {
        this.writer.WriteLine($"namespace {this.fullyQualifiedTargetNamespace} {{");
        this.writer.Indent();
    }

    private void StartClass(string targetClassName)
    {
        // Attributes to assist the debugger
        this.writer.WriteLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{this.selfFullyQualifiedTypeName}\", \"{this.selfVersion}\")]");
        this.writer.WriteLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
        this.writer.WriteLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
        this.writer.WriteLine($"{this.accessModifier} static class {targetClassName} {{");
        this.writer.Indent();

        // Resource Loader lazy field
        this.writer.WriteLine($"private static {FQ_RESOURCE_LOADER}? resourceLoader;");
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

        var isDefaultResourceMap = String.Equals(targetClassName, "Resources", StringComparison.InvariantCultureIgnoreCase);
        this.writer.WriteLine($"resourceLoader = new {FQ_RESOURCE_LOADER}({ (isDefaultResourceMap ? "" : $"\"{targetClassName}\"") });");
        
        this.writer.Dindent();
        this.writer.WriteLine("}");
        
        this.writer.WriteLine("return resourceLoader;");
        this.writer.Dindent();
        this.writer.WriteLine("}");

        // End Property
        this.writer.Dindent();
        this.writer.WriteLine("}");
    }

    private void WritePropertyAccessorForResource(string resourceName)
    {
        this.writer.WriteLine($"{accessModifier} string {resourceName} => Loader.GetString(\"{resourceName}\");");
    }

    private void EndClass() => this.EndNamespace();

    private void EndNamespace()
    {
        this.writer.Dindent();
        this.writer.WriteLine("}");
    }

}