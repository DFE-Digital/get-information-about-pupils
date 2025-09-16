using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.FilterRegistration;
using DfE.GIAP.Web.ViewModels.Search;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Filters;

public class FiltersRequestFactoryTests
{
    [Fact]
    public void GenerateFilterRequest_WithNullFilters_ReturnsEmptyDictionary()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new();

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, null!);

        // assert
        Assert.Empty(result);
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.IsAny<List<CurrentFilterDetail>>(), model, result), Times.Never);
    }

    [Fact]
    public void GenerateFilterRequest_WithEmptyFilters_ReturnsEmptyDictionary()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new();
        List<CurrentFilterDetail> filters = [];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        Assert.Empty(result);
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.IsAny<List<CurrentFilterDetail>>(), model, result), Times.Never);
    }

    [Fact]
    public void GenerateFilterRequest_WithSingleGenderValue_AppendsGenderFilter()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new()
        {
            SelectedGenderValues = ["Female"]
        };
        List<CurrentFilterDetail> filters = [];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.Is<List<CurrentFilterDetail>>(currentFilterDetails =>
                    currentFilterDetails.Any(currentFilterDetail =>
                        currentFilterDetail.FilterType == FilterType.Gender &&
                        currentFilterDetail.FilterName == "Female")),
                model, result),
                Times.Once);
    }

    [Fact]
    public void GenerateFilterRequest_WithMultipleGenderValues_DoesNotAppendGenderFilter()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new()
        {
            SelectedGenderValues = ["Male", "Female"]
        };
        List<CurrentFilterDetail> filters = [];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.Is<List<CurrentFilterDetail>>(currentFilterDetails =>
                    currentFilterDetails.Any(currentFilterDetail =>
                        currentFilterDetail.FilterType == FilterType.Gender)),
                model, result),
                Times.Never);
    }

    [Fact]
    public void GenerateFilterRequest_WithValidFilters_DelegatesToRegistry()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new();
        List<CurrentFilterDetail> filters =
        [
            new() { FilterName = "Smith", FilterType = FilterType.Surname }
        ];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(filters, model, result), Times.Once);
    }
}

