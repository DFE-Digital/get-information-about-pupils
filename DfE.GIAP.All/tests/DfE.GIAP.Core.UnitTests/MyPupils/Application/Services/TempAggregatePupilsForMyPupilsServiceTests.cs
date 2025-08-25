using Azure.Search.Documents;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
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
    public async Task GetPupilsAsync_NpdIndexIsEmpty_Calls_Both_Indexes_Returns_Combined_Results()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(20);
        List<AzureIndexEntity> pupilPremiumResults = AzureIndexEntityDtosTestDoubles.GenerateWithUpns(upns);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
        {
            { "pupil-premium", pupilPremiumResults}
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<DecoratedSearchIndexDto, Pupil>> mapper = MapperTestDoubles.MockFor<DecoratedSearchIndexDto, Pupil>();
        TempAggregatePupilsForMyPupilsApplicationService service = new(searchClientProviderMock.Object, mapper.Object);

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(upns);

        // Assert
        Assert.Equal(20, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }


    [Fact]
    public async Task GetPupilsAsync_PupilPremiumIndexIsEmpty_Calls_Both_Indexes_Returns_Combined_Results()
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
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetPupilsAsync_CallsBothIndexes_Returns_Combined_Results()
    {
        // Arrange
        List<AzureIndexEntity> npdResults = AzureIndexEntityDtosTestDoubles.Generate(100);
        List<AzureIndexEntity> ppResults = AzureIndexEntityDtosTestDoubles.Generate(199);

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
        Assert.Equal(299, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }
}
