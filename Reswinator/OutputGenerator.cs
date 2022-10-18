using System.Text;

namespace Codevoid.Utilities.Reswinator;

public class OutputGenerator
{
    private StringBuilder builder = new StringBuilder();
    private uint indentLevel = 0;

    public string GetOutput() => builder.ToString();

    public void Indent()
    {
        indentLevel += 1;
    }

    public void Dindent()
    {
        if(indentLevel < 1)
        {
            throw new NotSupportedException("You can't have an indent level less than zero");
        }

        indentLevel -= 1;
    }

    private void WriteIndent()
    {
        if(indentLevel < 1)
        {
            return;
        }

        for (var level = 0; level < indentLevel; level += 1)
        {
            builder.Append("    ");
        }
    }

    public void NewLine()
    {
        builder.AppendLine(String.Empty);
    }

    public void WriteLine(string line)
    {
        this.WriteIndent();
        builder.AppendLine(line);
    }
}