using Codevoid.Utilities.Reswinator;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Codevoid.Tests.Reswinator;

public class WrapperGeneratorTests
{
    private readonly WrapperGenerator generator = new WrapperGenerator("Codevoid.Test.Generated", "0.0.0.0");
    private readonly ITestOutputHelper output;

    public WrapperGeneratorTests(ITestOutputHelper output) => this.output = output;

    private string ReadContentsFor(string filename)
    {
        return File.ReadAllText(filename);
    }

    private void WriteOutputForTest(string contents, [CallerMemberName] string name = "")
    {
        var dir = Directory.CreateDirectory("output");;
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
        this.AssertHashesMatch("D623B9ABF0E3CAD20FFC984C6A004B8F5C8B93EE9FB03144BADBF7353EE09A97", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("E675ADF9A7A31CFD9ECB3EF6D8B041B77319FEF2D6A7476745199A715A36D5F7", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("D54BC92DECCAA2212902A5DA3CD620A9DCF9CE11CE691127A8A42B17350A31B0", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("9655316F8ACF1761EB578780D24D29A82DBD7355282C6DDC0E8F190F67BD0E2F", hashOfContents);
    }
}