using Azure.Search.Documents;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;
public sealed class AggregatePupilsForMyPupilsServiceTests
{
    [Fact]
    public async Task GetPupilsAsync_ReturnsEmpty_WhenNoUPNsProvided()
    {
        // Arrange
        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.Default();

        Mock<IMapper<AzureIndexEntityWithPupilType, Pupil>> mapper = MapperTestDoubles.MockFor<AzureIndexEntityWithPupilType, Pupil>();

        AggregatePupilsForMyPupilsApplicationService service = new(
            searchClientProviderMock.Object,
            mapper.Object,
            new Mock<IOrderPupilsHandler>().Object,
            new Mock<IPaginatePupilsHandler>().Object);

        UniquePupilNumbers uniquePupilNumbers = UniquePupilNumbers.Create([]);

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(uniquePupilNumbers, query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPupilsAsync_Throws_WhenUPNCountExceedsLimit()
    {
        // Arrange
        Mock<ISearchClientProvider> searchClientProviderMock = new();

        Mock<IMapper<AzureIndexEntityWithPupilType, Pupil>> mapper = MapperTestDoubles.MockFor<AzureIndexEntityWithPupilType, Pupil>();

        AggregatePupilsForMyPupilsApplicationService service = new(
            searchClientProviderMock.Object,
            mapper.Object,
            new Mock<IOrderPupilsHandler>().Object,
            new Mock<IPaginatePupilsHandler>().Object);

        const int exceedsLimit = 4001;
        UniquePupilNumbers uniquePupilNumbers =
            UniquePupilNumbers.Create(
                UniquePupilNumberTestDoubles.Generate(exceedsLimit));

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetPupilsAsync(
                uniquePupilNumbers,
                It.Is<MyPupilsQueryModel>((q) => ReferenceEquals(q, query)),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetPupilsAsync_NpdIndexIsEmpty_Calls_Both_Indexes_Returns_Combined_Results()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> generatedUpns = UniquePupilNumberTestDoubles.Generate(20);

        UniquePupilNumbers uniquePupilNumbers = UniquePupilNumbers.Create(generatedUpns);

        List<AzureIndexEntity> pupilPremiumResults = AzureIndexEntityDtosTestDoubles.GenerateWithUpns(generatedUpns);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
        {
            { "pupil-premium", pupilPremiumResults}
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<AzureIndexEntityWithPupilType, Pupil>> mapper = MapperTestDoubles.MockFor<AzureIndexEntityWithPupilType, Pupil>();

        Mock<IOrderPupilsHandler> orderPupilsMock = new();
        orderPupilsMock
            .Setup((t)
                => t.Order(
                    It.IsAny<IEnumerable<Pupil>>(), It.IsAny<OrderOptions>()))
            .Returns([]);

        const int outputPupilsCount = 13;
        Mock<IPaginatePupilsHandler> paginatePupilsMock = new();
        paginatePupilsMock
            .Setup((t)
                => t.PaginatePupils(
                    It.IsAny<IEnumerable<Pupil>>(), It.IsAny<PaginationOptions>()))
            .Returns(PupilTestDoubles.Generate(outputPupilsCount));

        AggregatePupilsForMyPupilsApplicationService service = new(
            searchClientProviderMock.Object,
            mapper.Object,
            orderPupilsMock.Object,
            paginatePupilsMock.Object);

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(
            uniquePupilNumbers,
            query,
            It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(outputPupilsCount, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }


    [Fact]
    public async Task GetPupilsAsync_PupilPremiumIndexIsEmpty_Calls_Both_Indexes_Returns_Combined_Results()
    {
        // Arrange
        IEnumerable<UniquePupilNumber> generatedUpns = UniquePupilNumberTestDoubles.Generate(20);
        UniquePupilNumbers uniquePupilNumbers = UniquePupilNumbers.Create(generatedUpns);

        List<AzureIndexEntity> npdResults = AzureIndexEntityDtosTestDoubles.GenerateWithUpns(generatedUpns);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
        {
            { "npd", npdResults}
        };

        Mock<ISearchClientProvider> searchClientProviderMock = SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<AzureIndexEntityWithPupilType, Pupil>> mapper = MapperTestDoubles.MockFor<AzureIndexEntityWithPupilType, Pupil>();

        Mock<IOrderPupilsHandler> orderPupilsMock = new();
        orderPupilsMock
            .Setup((t)
                => t.Order(
                    It.IsAny<IEnumerable<Pupil>>(), It.IsAny<OrderOptions>()))
            .Returns([]);

        const int outputPupilsCount = 13;
        Mock<IPaginatePupilsHandler> paginatePupilsMock = new();
        paginatePupilsMock
            .Setup((t)
                => t.PaginatePupils(
                    It.IsAny<IEnumerable<Pupil>>(), It.IsAny<PaginationOptions>()))
            .Returns(PupilTestDoubles.Generate(outputPupilsCount));

        AggregatePupilsForMyPupilsApplicationService service = new(
            searchClientProviderMock.Object,
            mapper.Object,
            orderPupilsMock.Object,
            paginatePupilsMock.Object);

        MyPupilsQueryModel query = MyPupilsQueryModel.CreateDefault();

        // Act
        IEnumerable<Pupil> result = await service.GetPupilsAsync(
            uniquePupilNumbers,
            query,
            It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(outputPupilsCount, result.Count());
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("npd", It.IsAny<SearchOptions>()), Times.Once);
        searchClientProviderMock.Verify(p => p.InvokeSearchAsync<AzureIndexEntity>("pupil-premium", It.IsAny<SearchOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetPupilsAsync_Maps_Dedupes_Orders_Then_Paginates_InSequence_And_Passes_Correct_Arguments()
    {
        IEnumerable<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(6);
        // Make two overlap sets: first 4 appear in both, last 2 only in npd
        List<UniquePupilNumber> overlappedUpnsInIndexes = upns.Take(4).ToList();
        List<UniquePupilNumber> npdOnly = upns.Skip(4).Take(2).ToList();

        List<AzureIndexEntity> npdResults =
            AzureIndexEntityDtosTestDoubles.GenerateWithUpns(overlappedUpnsInIndexes.Concat(npdOnly));

        List<AzureIndexEntity> ppResults =
            AzureIndexEntityDtosTestDoubles.GenerateWithUpns(overlappedUpnsInIndexes);

        Dictionary<string, List<AzureIndexEntity>> searchClientKeyToResponseData = new()
            {
                { "npd", npdResults },
                { "pupil-premium", ppResults }
            };

        Mock<ISearchClientProvider> searchClientProviderMock =
            SearchClientProviderTestDoubles.MockFor(searchClientKeyToResponseData);

        Mock<IMapper<AzureIndexEntityWithPupilType, Pupil>> mapper =
            MapperTestDoubles.MockFor<AzureIndexEntityWithPupilType, Pupil>();

        mapper
            .Setup(m => m.Map(It.IsAny<AzureIndexEntityWithPupilType>()))
            .Returns<AzureIndexEntityWithPupilType>(dto =>
            {
                UniquePupilNumber upn = new UniquePupilNumber(dto.SearchIndexDto.UPN);
                Pupil pupil = PupilBuilder
                    .CreateBuilder(upn)
                    .WithFirstName(dto.SearchIndexDto.Forename ?? "F")
                    .WithSurname(dto.SearchIndexDto.Surname ?? "S")
                    .WithDateOfBirth(dto.SearchIndexDto.DOB)
                    .WithSex(null)
                    .Build();
                return pupil;
            });

        Mock<IOrderPupilsHandler> orderHandlerMock = new();
        Mock<IPaginatePupilsHandler> paginateHandlerMock = new();

        List<Pupil>? orderedInputObserved = null;

        List<Pupil> orderedPupils = PupilTestDoubles.Generate(count: 5);

        List<Pupil> pagedPupils = PupilTestDoubles.Generate(count: 3);

        MyPupilsQueryModel query = new(
            pageNumber: 2,
            size: 10,
            orderBy: ("surname", "asc"));

        MockSequence sequence = new();

        orderHandlerMock
            .InSequence(sequence)
            .Setup(o => o.Order(
                It.IsAny<IEnumerable<Pupil>>(),
                It.Is<OrderOptions>(oo => ReferenceEquals(oo, query.Order))))
            .Callback<IEnumerable<Pupil>, OrderOptions>((input, _) => orderedInputObserved = input.ToList())
            .Returns(orderedPupils);

        paginateHandlerMock
            .InSequence(sequence)
            .Setup(p => p.PaginatePupils(
                It.Is<IEnumerable<Pupil>>(arg => ReferenceEquals(arg, orderedPupils)),
                It.Is<PaginationOptions>(po => ReferenceEquals(po, query.PaginateOptions))))
            .Returns(pagedPupils);

        AggregatePupilsForMyPupilsApplicationService sut = new(
            searchClientProviderMock.Object,
            mapper.Object,
            orderHandlerMock.Object,
            paginateHandlerMock.Object);

        UniquePupilNumbers request = UniquePupilNumbers.Create(upns);

        IEnumerable<Pupil> result = await sut.GetPupilsAsync(request, query, CancellationToken.None);

        // Assert
        orderHandlerMock.Verify(o => o.Order(
                It.IsAny<IEnumerable<Pupil>>(),
                It.Is<OrderOptions>(oo => ReferenceEquals(oo, query.Order))),
            Times.Once);

        paginateHandlerMock.Verify(p => p.PaginatePupils(
                It.Is<IEnumerable<Pupil>>(arg => ReferenceEquals(arg, orderedPupils)),
                It.Is<PaginationOptions>(po => ReferenceEquals(po, query.PaginateOptions))),
            Times.Once);

        Assert.Same(pagedPupils, result);


        // verify input to OrderHandler
        List<AzureIndexEntityWithPupilType> allDecorated =
            npdResults
                .Select(x => new AzureIndexEntityWithPupilType(x, PupilType.NationalPupilDatabase))
                .Concat(ppResults.Select(x => new AzureIndexEntityWithPupilType(x, PupilType.PupilPremium)))
                .ToList();

        List<AzureIndexEntityWithPupilType> expectedChosenDecorated =
            allDecorated
                .GroupBy(d => d.SearchIndexDto.UPN)
                .Select(g => g.OrderByDescending(x => x.PupilType == PupilType.PupilPremium).First())
                .ToList();

        // Map using the same lambda the mapper mock uses, so sequence/content match
        List<Pupil> expectedOrderInput = expectedChosenDecorated
            .Select(d =>
                PupilBuilder
                    .CreateBuilder(new UniquePupilNumber(d.SearchIndexDto.UPN))
                    .WithFirstName(d.SearchIndexDto.Forename ?? "F")
                    .WithSurname(d.SearchIndexDto.Surname ?? "S")
                    .WithDateOfBirth(d.SearchIndexDto.DOB)
                    .WithSex(null)
                    .Build())
            .ToList();

        Assert.NotNull(orderedInputObserved);
        Assert.Equal(expectedOrderInput, orderedInputObserved);
    }
}
