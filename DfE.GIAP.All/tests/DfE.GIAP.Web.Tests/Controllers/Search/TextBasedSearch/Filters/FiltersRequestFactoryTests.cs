using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Filters.FilterRegistration;
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
    public void GenerateFilterRequest_WithSingleSexValue_AppendsGenderFilter()
    {
        // arrange
        Mock<IFilterHandlerRegistry> registryMock = new();
        FiltersRequestFactory factory = new(registryMock.Object);
        LearnerTextSearchViewModel model = new()
        {
            SelectedSexValues = ["Female"]
        };
        List<CurrentFilterDetail> filters = [];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.Is<List<CurrentFilterDetail>>(currentFilterDetails =>
                    currentFilterDetails.Any(currentFilterDetail =>
                        currentFilterDetail.FilterType == FilterType.Sex &&
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
            SelectedSexValues = ["Male", "Female"]
        };
        List<CurrentFilterDetail> filters = [];

        // act
        Dictionary<string, string[]> result = factory.GenerateFilterRequest(model, filters);

        // assert
        registryMock.Verify(filterHandlerRegistry =>
            filterHandlerRegistry.ApplyFilters(
                It.Is<List<CurrentFilterDetail>>(currentFilterDetails =>
                    currentFilterDetails.Any(currentFilterDetail =>
                        currentFilterDetail.FilterType == FilterType.Sex)),
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

