
using System.Xml;

namespace Codevoid.Utilities.Reswinator;

/// <summary>
/// Generates source code to access resources in a supplied ResW content
/// </summary>
internal class WrapperGenerator
{
    private static readonly string FQ_RESOURCE_LOADER = "global::Microsoft.Windows.ApplicationModel.Resources.ResourceLoader";
    private OutputWriter writer = new OutputWriter();
    private string fullyQualifiedTargetNamespace = "Codevoid.Sample";
    private string targetClassName = "Resources";
    private string accessModifier = "internal";

    /// <summary>
    /// For the supplied ResW contents, generates an accessor class that
    /// provides static, strongly typed access to the resources in that ResW
    /// </summary>
    /// <param name="contents">The contents of the ResW have to be wrapped</param>
    /// <param name="assumedResourceMap">
    /// The assumed name of the resource map that will be be read from at
    /// runtime
    /// </param>
    /// <returns>Source code for the wrapper</returns>
    internal string GenerateWrapperForResw(string contents, string assumedResourceMap)
    {
        var reswXml = new XmlDocument();
        reswXml.LoadXml(contents);

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
        this.StartClass();

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

    private void StartClass()
    {

        // Attributes to assist the debugger
        this.writer.WriteLine("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"MY NAMESPACE OF STUFF\", \"15.1.0.0\")]");
        this.writer.WriteLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
        this.writer.WriteLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
        this.writer.WriteLine($"{this.accessModifier} static sealed class {this.targetClassName} {{");
        this.writer.Indent();

        // Resource Loader lazy field
        this.writer.WriteLine($"private static {FQ_RESOURCE_LOADER}? resourceLoader;");
        this.writer.NewLine();

        // Resource Loader property declaration
        this.writer.WriteLine($"private static {FQ_RESOURCE_LOADER} Loader {{");
        this.writer.Indent();

        // Resource Loader property getter
        this.writer.WriteLine("get {");
        this.writer.Indent();

        // Resource Loader null check, instantiation, and return
        this.writer.WriteLine("if (resourceLoader is null) {");
        this.writer.Indent();
        
        this.writer.WriteLine($"resourceLoader = new {FQ_RESOURCE_LOADER}({ (targetClassName == "Resources" ? "" : $"\"{targetClassName}") });");
        
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