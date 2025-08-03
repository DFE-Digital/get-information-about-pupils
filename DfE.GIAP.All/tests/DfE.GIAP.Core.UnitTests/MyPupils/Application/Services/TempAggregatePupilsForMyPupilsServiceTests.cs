using Azure.Search.Documents;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;
public sealed class TempAggregatePupilsForMyPupilsServiceTests
{
    [Fact]
    public async Task GetPupilsAsync_ReturnsEmpty_WhenNoUPNsProvided()
    {
        // Arrange
        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.Default();
        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPupilsAsync_Throws_WhenUPNCountExceedsLimit()
    {
        // Arrange
        Mock<ISearchClientProvider> searchClientProviderMock = new();
        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(4001);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetPupilsAsync(upns));
    }

    [Fact]
    public async Task GetPupilsAsync_ReturnsMappedNpdResults_WhenNpdResultsReachPageSize()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(20);
        List<AzureIndexEntity> npdResults = AzureIndexEntityDtosTestDoubles.GenerateWithUpns(upns);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
        {
            { "npd", npdResults}
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(upns);

        // Assert
        Assert.Equal(20, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Never);
    }

    [Fact]
    public async Task GetPupilsAsync_CallsBothIndexes_WhenNpdResultsAreLessThanPageSize()
    {
        // Arrange
        List<AzureIndexEntity> npdResults = AzureIndexEntityDtosTestDoubles.Generate(10);
        List<AzureIndexEntity> ppResults = AzureIndexEntityDtosTestDoubles.Generate(15);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
        {
            { "npd", npdResults},
            { "pupil-premium", ppResults }
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        List<UniquePupilNumber> requestUpns =
            npdResults.Concat(ppResults)
                .Select(t => t.UPN)
                .ToUniquePupilNumbers()
                .ToList();

        IEnumerable<Pupil> result = await service.GetPupilsAsync(requestUpns);

        // Assert
        Assert.Equal(20, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetPupilsAsync_SkipsCorrectNumberOfUpns_BasedOnPageNumber()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(40); // 2 pages worth

        MyPupilsQueryOptions queryOptions = new(
            new OrderPupilsBy("", SortDirection.Descending),
            PageNumber.Page(2));

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToSearchIndexResponseDtos = new()
        {
            { "npd", AzureIndexEntityDtosTestDoubles.GenerateWithUpns(upns.Skip(20).Take(20)) }
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToSearchIndexResponseDtos);
        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(upns, queryOptions);

        // Assert
        Assert.Equal(20, result.Count());

        IEnumerable<string> expectedUpnsFilteredFor = upns.Skip(20).Select(t => t.Value);

        searchClientProviderMock.Verify(
            (provider) => provider.InvokeSearchAsync<AzureIndexEntity>(
                "npd",
                It.Is<SearchOptions>(
                    (options) => expectedUpnsFilteredFor.All(t => options.Filter.Contains(t)))), Times.Once);
    }

    [Fact]
    public async Task GetPupilsAsync_ReturnsEmpty_WhenPagedUpnsCountIsZero()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(10); // Less than one full page
        MyPupilsQueryOptions queryOptions = new(
            Order: new OrderPupilsBy(string.Empty, SortDirection.Descending),
            Page: PageNumber.Page(2)); // Page 2 skips all 10

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.Default();
        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(upns, queryOptions);

        // Assert
        Assert.Empty(result);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>(It.IsAny<string>(), It.IsAny<SearchOptions>()), Times.Never);
    }


    [Theory]
    [InlineData("Surname", SortDirection.Ascending, "Surname asc")]
    [InlineData("Surname", SortDirection.Descending, "Surname desc")]
    [InlineData("Forename", SortDirection.Ascending, "Forename asc")]
    [InlineData("Forename", SortDirection.Descending, "Forename desc")]
    [InlineData("Sex", SortDirection.Ascending, "Sex asc")]
    [InlineData("Sex", SortDirection.Descending, "Sex desc")]
    [InlineData("DOB", SortDirection.Ascending, "DOB asc")]
    [InlineData("DOB", SortDirection.Descending, "DOB desc")]
    [InlineData("UnknownField ", SortDirection.Descending, "search.score() desc")]
    [InlineData(" ", SortDirection.Ascending, "search.score() asc")]
    public void CreateSearchClientOptions_RespectsSortDirection(string inputSortField, SortDirection direction, string expectedOrderBy)
    {
        // Arrange
        List<UniquePupilNumber> upn = UniquePupilNumberTestDoubles.Generate(1);

        // Act
        SearchOptions options = TempAggregatePupilsForMyPupilsApplicationService.CreateSearchClientOptions(
            upn,
            inputSortField,
            direction);

        // Assert
        Assert.Equal(expectedOrderBy, options.OrderBy.Single());
    }
}
