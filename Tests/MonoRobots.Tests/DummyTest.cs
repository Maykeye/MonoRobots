using Xunit;
using Shouldly;
using MonoRobots;

namespace MonoRobots.Tests;

public class DummyTest
{
    [Fact]
    public void TestMethod()
    {
        Foo.Bar.ShouldBe(2, "Implement the tests");
    }
}

