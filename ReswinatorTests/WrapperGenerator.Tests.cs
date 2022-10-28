using Codevoid.Utilities.Reswinator;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Codevoid.Test.Reswinator;

public class WrapperGeneratorTests
{
    private readonly WrapperGenerator generator = new WrapperGenerator("Codevoid.Test.Generated", NullableState.Enabled);
    private readonly ITestOutputHelper output;

    public WrapperGeneratorTests(ITestOutputHelper output) => this.output = output;

    private string ReadContentsFor(string filename)
    {
        return File.ReadAllText(filename);
    }

    private void TraceOutputToDisk(string contents, [CallerMemberName] string name = "")
    {
        var dir = Directory.CreateDirectory("output"); ;
        File.WriteAllText($"{dir}/{name}.cs", contents);
    }

    private string HashContents(string toHash)
    {
        // Make the output consistent across windows + other platforms by
        // normalizing the line endings
        toHash = toHash.Replace("\r\n", "\n");
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(toHash)));
        return Convert.ToHexString(hash);
    }

    private void AssertHashesMatch(string expected, string actual)
    {
        this.output.WriteLine($"Computed Hash: {actual}");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GeneratingForEmptyFileGeneratesNoOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/Empty.resw"), "Resources");
        Assert.Empty(wrapperSource);
    }

    [Fact]
    public void GeneratingWithSingleItemMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("571742A82C61A86C2C11B191DBDC35D6E72AFAD3C4F926806E2FA056377F630B", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemNullableDisabledMatchesExpectedOutput()
    {
        var alternativeGenerator = new WrapperGenerator("Codevoid.Test.Generated.NullableDisabled", NullableState.Disabled);
        var wrapperSource = alternativeGenerator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("756FA8ECF7467D7D646788E7844F305CB94DC6A83E910F2772D28F960B3070B3", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "CustomResourceMap");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("59E08A0544BD61A11823F269815C5B4C47911379E681B5B018560BFADC29AF4F", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleResourcesMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("E51DCDBDCF149676938DD870CDB98ABF417881295A1534416DF4AF5D36317F60", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleResourcesMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "CustomResourceMap");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("B1D74E845A7D3E88E3844834D394EA8B77B1142026BB39E9C584FC214829D552", hashOfContents);
    }

    [Fact]
    public void GeneratingWithNestedResourcesMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/NestedResources.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("B76813765A506E652F11FC35565D5E61FDFB1BA18F98C14F48783029BCCF232B", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleFilesWithSingleGenerator()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("E51DCDBDCF149676938DD870CDB98ABF417881295A1534416DF4AF5D36317F60", hashOfContents);

        wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.TraceOutputToDisk(wrapperSource);
        hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("E51DCDBDCF149676938DD870CDB98ABF417881295A1534416DF4AF5D36317F60", hashOfContents);
    }
}