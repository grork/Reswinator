using Codevoid.Utilities.Reswinator;

namespace Codevoid.Tests.Reswinator;

public class OutputGeneratorTests
{
    private OutputGenerator generator = new OutputGenerator();

    [Fact]
    public void GettingOutputWithoutWritingAnythingGivesEmptyString()
    {
        Assert.Empty(this.generator.GetOutput());
    }

    [Fact]
    public void IndentingButNotWritingReturnsEmptyString()
    {
        this.generator.Indent();
        Assert.Empty(this.generator.GetOutput());
    }

    [Fact]
    public void DindentingWithNoIndentThrowsException()
    {
        Assert.Throws<NotSupportedException>(() => this.generator.Dindent());
    }

    [Fact]
    public void WritingLineAddsToOutput()
    {
        this.generator.WriteLine("Hello");
        Assert.Equal("Hello\n", this.generator.GetOutput());
    }

    [Fact]
    public void WritingLineWithIndentIncludesIndentBeforeWrittenLine()
    {
        this.generator.Indent();
        this.generator.WriteLine("Hello");
        Assert.Equal("    Hello\n", this.generator.GetOutput());
    }

    [Fact]
    public void WritingLineWithTwoIndentLevelsIncludesIndentBeforeWrittenLine()
    {
        this.generator.Indent();
        this.generator.Indent();
        this.generator.WriteLine("Hello");
        Assert.Equal("        Hello\n", this.generator.GetOutput());
    }

    [Fact]
    public void IndentLevevelChangesInterleavedWithWritesAreRespected()
    {
        this.generator.Indent();
        this.generator.Indent();
        this.generator.WriteLine("Hello");
        this.generator.Dindent();
        this.generator.Dindent();
        this.generator.WriteLine("World");
        Assert.Equal("        Hello\nWorld\n", this.generator.GetOutput());
    }

    [Fact]
    public void WritingNewLineOnlyAddsOnlyNewLineToOutput()
    {
        this.generator.NewLine();
        Assert.Equal("\n", this.generator.GetOutput());
    }

    [Fact]
    public void WritingNewLineWithIndentOnlyAddsOnlyNewLineToOutput()
    {
        this.generator.Indent();
        this.generator.NewLine();
        Assert.Equal("\n", this.generator.GetOutput());
    }
}
