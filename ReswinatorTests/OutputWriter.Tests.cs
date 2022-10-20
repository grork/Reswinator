using Codevoid.Utilities.Reswinator;

namespace Codevoid.Test.Reswinator;

public class OutputWriterTests
{
    private OutputWriter writer = new OutputWriter();

    [Fact]
    public void GettingOutputWithoutWritingAnythingGivesEmptyString()
    {
        Assert.Empty(this.writer.GetOutput());
    }

    [Fact]
    public void IndentingButNotWritingReturnsEmptyString()
    {
        this.writer.Indent();
        Assert.Empty(this.writer.GetOutput());
    }

    [Fact]
    public void DindentingWithNoIndentThrowsException()
    {
        Assert.Throws<NotSupportedException>(() => this.writer.Dindent());
    }

    [Fact]
    public void WritingLineAddsToOutput()
    {
        this.writer.WriteLine("Hello");
        Assert.Equal("Hello\n", this.writer.GetOutput());
    }

    [Fact]
    public void WritingLineWithIndentIncludesIndentBeforeWrittenLine()
    {
        this.writer.Indent();
        this.writer.WriteLine("Hello");
        Assert.Equal("    Hello\n", this.writer.GetOutput());
    }

    [Fact]
    public void WritingLineWithTwoIndentLevelsIncludesIndentBeforeWrittenLine()
    {
        this.writer.Indent();
        this.writer.Indent();
        this.writer.WriteLine("Hello");
        Assert.Equal("        Hello\n", this.writer.GetOutput());
    }

    [Fact]
    public void IndentLevevelChangesInterleavedWithWritesAreRespected()
    {
        this.writer.Indent();
        this.writer.Indent();
        this.writer.WriteLine("Hello");
        this.writer.Dindent();
        this.writer.Dindent();
        this.writer.WriteLine("World");
        Assert.Equal("        Hello\nWorld\n", this.writer.GetOutput());
    }

    [Fact]
    public void WritingNewLineOnlyAddsOnlyNewLineToOutput()
    {
        this.writer.NewLine();
        Assert.Equal("\n", this.writer.GetOutput());
    }

    [Fact]
    public void WritingNewLineWithIndentOnlyAddsOnlyNewLineToOutput()
    {
        this.writer.Indent();
        this.writer.NewLine();
        Assert.Equal("\n", this.writer.GetOutput());
    }
}
