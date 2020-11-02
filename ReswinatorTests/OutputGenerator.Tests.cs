using Codevoid.Utilities.Reswinator;

namespace Codevoid.Tests.Reswinator;

public class OutputGeneratorTests
{
    [Fact]
    public void CanInstantiate()
    {
        var outputGenerator = new OutputGenerator();
        Assert.NotNull(outputGenerator);
    }
}
