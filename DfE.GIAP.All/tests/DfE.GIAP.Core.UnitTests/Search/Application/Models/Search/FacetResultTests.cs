using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class FacetResultTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        string value = "North";
        long? count = 42;

        // act
        FacetResult result = new(value, count);

        // Assert
        result.Value.Should().Be(value);
        result.Count.Should().Be(count);
    }

    [Fact]
    public void Constructor_WithNullCount_ShouldAllowNull()
    {
        // arrange
        string value = "Unspecified";
        long? count = null;

        // act
        FacetResult result = new(value, count);

        // Assert
        result.Value.Should().Be(value);
        result.Count.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyValue_ShouldAllowEmptyString()
    {
        // act
        FacetResult result = new(string.Empty, 0);

        // Assert
        result.Value.Should().BeEmpty();
        result.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNullValue_ShouldAllowNull()
    {
        // act
        FacetResult result = new(null!, 10);

        // Assert
        result.Value.Should().BeNull();
        result.Count.Should().Be(10);
    }
}

