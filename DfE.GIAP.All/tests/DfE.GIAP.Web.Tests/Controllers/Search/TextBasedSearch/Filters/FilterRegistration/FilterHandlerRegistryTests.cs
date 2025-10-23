using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Filters.FilterRegistration;

public class FilterHandlerRegistryTests
{
    [Fact]
    public void Constructor_WithNullHandlers_ThrowsArgumentNullException()
    {
        // act/assert
        ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(() => new FilterHandlerRegistry(null!));

        Assert.Contains("handlers", ex.ParamName);
    }

    [Fact]
    public void ApplyFilters_WithMatchingHandler_CallsHandlerApply()
    {
        // arrange
        Mock<IFilterHandler> mockHandler = new();
        Dictionary<string, IFilterHandler> handlers = new()
        {
            { "SurnameLC", mockHandler.Object }
        };

        FilterHandlerRegistry registry = new(handlers);
        CurrentFilterDetail filter = new() { FilterName = "Surname", FilterType = FilterType.Surname };
        List<CurrentFilterDetail> filters = [filter];
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = [];

        // act
        registry.ApplyFilters(filters, model, requestFilters);

        // assert
        mockHandler.Verify(filterHandler =>
            filterHandler.Apply(filter, model, requestFilters), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithUnregisteredFilterType_DoesNotThrowOrApply()
    {
        // arrange
        FilterHandlerRegistry registry = new([]);
        CurrentFilterDetail filter = new() { FilterName = "UnknownFilter" };
        List<CurrentFilterDetail> filters = [filter];
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = [];

        // act/assert
        registry.ApplyFilters(filters, model, requestFilters); // Should silently skip

        // assert
        Assert.Null(model.SearchText); // assumes nothing changes
    }

    [Theory]
    [InlineData("Surname", "SurnameLC")]
    [InlineData("Forename", "ForenameLC")]
    [InlineData("Dob", "DOB")]
    [InlineData("Region", "Region")] // unmapped fallback
    public void NormalizeFilterType_ReturnsExpectedCanonicalKey(string input, string expected)
    {
        // Act
        System.Reflection.MethodInfo method =
            typeof(FilterHandlerRegistry)
                .GetMethod(
                    "NormalizeFilterType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        object? result = method.Invoke(null, [input]);

        // Assert
        Assert.Equal(expected, result);
    }
}

