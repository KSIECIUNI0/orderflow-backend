using FluentAssertions;

namespace OrderFlow.UnitTests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Test()
    {
        const bool result = false;

        result.Should().BeTrue();
    }
}