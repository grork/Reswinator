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
        this.AssertHashesMatch("905AED325CE975D09B5E137CCA42B3E321B5DC0983A93A662581A41021123C12", hashOfContents);
    }

    [Fact]
    public void GeneratingWithSingleItemMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/SingleResource.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("8DA4C40C194C41E03A68C091E476AAD25ADE240568D76A65BCB120D9CB84270F", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutput()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "Resources");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("3E614B7A863E4371E064B20C4B8275F2A4FC86CFD4D273CB8FDC6CED43A584E1", hashOfContents);
    }

    [Fact]
    public void GeneratingForMultipleItemsMatchesExpectedOutputNonDefaultResourceMap()
    {
        var wrapperSource = generator.GenerateWrapperForResw(this.ReadContentsFor("samples/MultipleResources.resw"), "CustomResourceMap");
        this.WriteOutputForTest(wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        this.AssertHashesMatch("75912AC7503875CF4959DB2C71D69A11B52F9B89E9F6DF571333A00E8FB56D35", hashOfContents);
    }
}