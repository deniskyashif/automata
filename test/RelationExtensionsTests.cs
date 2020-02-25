using System.Linq;
using Xunit;

public class RelationExtensionsTests
{
    [Fact]
    public void ComposeBinaryRelTest()
    {
        var r1 = new[] { (1, 2), (2, 3), (4, 5) };
        var r2 = new[] { (2, 9), (5, 5) };
        var expected = new[] { (1, 9), (4, 5) };

        Assert.Equal(expected, r1.Compose(r2));
    }

    [Fact]
    public void ComposeBinaryRelTest1()
    {
        var r1 = new[] { (2, 1), (3, 3) };
        var r2 = new[] { (2, 9), (5, 5) };

        Assert.Empty(r1.Compose(r2));
    }

    [Fact]
    public void ComposeBinaryRelTest2()
    {
        var r1 = new[] { (2, 1), (3, 3) };
        var r2 = new[] { (1, 9), (5, 5) };

        Assert.Equal(new[] { (2, 9) }, r1.Compose(r2));
    }

    [Fact]
    public void TransitiveClosureTest()
    {
        var r = new[] { (1, 2), (2, 3), (4, 5) };
        var actual = r.TransitiveClosure().OrderBy(x => x.Item1).ThenBy(x => x.Item2);
        var expected = new[] { (1, 2), (2, 3), (4, 5), (1, 3) }
            .OrderBy(x => x.Item1)
            .ThenBy(x => x.Item2);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TransitiveClosureTest1()
    {
        var r = new[] { (1, 2), (2, 3), (3, 4), (4, 5) };
        var actual = r.TransitiveClosure().OrderBy(x => x.Item1).ThenBy(x => x.Item2);
        var expected = new[]
        {
            (1, 2), (2, 3), (3, 4), (4, 5), (1, 3), (2, 4), (3,5), (1, 4), (2, 5), (1, 5)
        }.OrderBy(x => x.Item1).ThenBy(x => x.Item2);

        Assert.Equal(expected, actual);
    }
}