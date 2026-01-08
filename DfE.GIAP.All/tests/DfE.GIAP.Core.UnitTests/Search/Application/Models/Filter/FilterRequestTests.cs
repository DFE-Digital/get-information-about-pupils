using DfE.GIAP.Core.Search.Application.Models.Filter;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Filter;

public class FilterRequestTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        string filterName = "Region";
        List<object> filterValues = ["North", "South"];

        // act
        FilterRequest request = new(filterName, filterValues);

        // Assert
        request.FilterName.Should().Be(filterName);
        request.FilterValues.Should().BeEquivalentTo(filterValues);
    }

    [Fact]
    public void Constructor_WithNullFilterName_ShouldThrowArgumentNullException()
    {
        // arrange
        IList<object> values = ["Value1"];

        // act
        Action act = () => new FilterRequest(null!, values);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filterName");
    }

    [Fact]
    public void Constructor_WithNullFilterValues_ShouldThrowArgumentNullException()
    {
        // act
        Action act = () => new FilterRequest("Field", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("filterValues");
    }

    [Fact]
    public void FilterValues_ShouldBeReadOnly()
    {
        // arrange
        List<object> values = ["A", "B"];
        FilterRequest request = new("Field", values);

        // act
        IList<object> readOnlyValues = request.FilterValues;

        // Assert
        readOnlyValues.IsReadOnly.Should().BeTrue();
        Action mutate = () => ((List<object>)readOnlyValues).Add("C");
        mutate.Should().Throw<InvalidCastException>(); // ReadOnlyCollection cannot be cast to List
    }
}

