using Codevoid.Utilities.Reswinator;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Codevoid.Test.Reswinator;

public class WrapperGeneratorTests
{
    private readonly WrapperGenerator generator = new WrapperGenerator("Codevoid.Test.Generated", NullableState.Enabled, "0.0.0.0");
    private readonly ITestOutputHelper output;

    public WrapperGeneratorTests(ITestOutputHelper output) => this.output = output;

    private string ReadContentsFor(string filename)
    {
        return File.ReadAllText(filename);
    }

    private void WriteOutputForTest(string contents, [CallerMemberName] string name = "")
    {
        var dir = Directory.CreateDirectory("output"); ;
        File.WriteAllText($"{dir}/{name}.cs", contents);
    }

    private string HashContents(string toHash)
    {
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
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("BAC4898722D4163702CE2BD86C106781B4087CEACF11E2B97D69D73B51743193", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemNullableDisabledMatchesExpectedOutput()
    {
        var alternativeGenerator = new WrapperGenerator("Codevoid.Test.Generated.NullableDisabled", NullableState.Disabled, "0.0.0.0");
        var wrapperSource = alternativeGenerator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "Resources");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("BC498448988C4D5C46A8BEA8618357BDFB300880C43472B0B369C644F5139D6B", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("74B9E505D565C25C0428B3204A852B65B7F6EC14BA77A6021A57F8B7406A6C79", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("3EC1174921E5D09203EF2399EDFD9F0A152BFF8842EA519CB3546FDA817E1BA9", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("84DDD5C965E918D733E22F481AAC0B7C0E0925A4E77BAAD480EDD0A1ECC0D8BC", hashOfContents);
    }

    [Fact]
    public void GeneratingWithNestedResourcesMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/NestedResources.resw"), "Resources");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("82FB19DF61BABA0565704EDF84DECDC67EA2FEA67EAD43111FDD9CD1C39C4C55", hashOfContents);
    }
}