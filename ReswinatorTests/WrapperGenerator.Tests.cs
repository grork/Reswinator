using Codevoid.Utilities.Reswinator;
using System.Security.Cryptography;
using System.Text;

namespace Codevoid.Tests.Reswinator;

public class WrapperGeneratorTests
{
    private WrapperGenerator generator = new WrapperGenerator();

    private string ReadContentsFor(string filename)
    {
        return File.ReadAllText(filename);
    }

    private void WriteOutputForTest(string name, string contents)
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
        this.WriteOutputForTest(nameof(GeneratingWithSingleItemMatchesExpectedOutput), wrapperSource);
        var hashOfContents = this.HashContents(wrapperSource);
        Assert.Equal("8AB44F119C2B74877421CC578ACB07D6CB6407EA7925B75222591B2AC96EC9DF", hashOfContents);
    }
}