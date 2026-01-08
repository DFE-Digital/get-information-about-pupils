using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Features.MyPupils.DataTransferObjects;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly GiapCosmosDbFixture _cosmosDbFixture;
    private const string MyPupilsContainerName = "mypupils";

    private MyPupilsTestContext? _testContext;
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(GiapCosmosDbFixture cosmosDbFixture)
    {
        _cosmosDbFixture = cosmosDbFixture;
    }

    protected async override Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.ClearDatabaseAsync());

        services.AddMyPupilsCore();

        // Initialise fixture and pupils, store in context
        List<AzureNpdSearchResponseDto> npdSearchindexDtos = AzureNpdSearchResponseDtoTestDoubles.Generate(count: 10);
        List<AzureNpdSearchResponseDto> pupilPremiumIndexDtos = AzureNpdSearchResponseDtoTestDoubles.Generate(count: 10);

        List<string> myPupilsUpns =
            npdSearchindexDtos.Concat(pupilPremiumIndexDtos)
                .Select(t => t.UPN)
                .ToList();

        MyPupilsId myPupilId = MyPupilsIdTestDoubles.Default();

        MyPupilsDocumentDto myPupilsDocument =
            MyPupilsDocumentDtoTestDoubles.Create(
                myPupilId,
                UniquePupilNumbers.Create(
                    myPupilsUpns.Select(t => new UniquePupilNumber(t))));

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.WriteItemAsync(containerName: MyPupilsContainerName, myPupilsDocument));

        _testContext = new MyPupilsTestContext(myPupilId.Value, myPupilsUpns);
    }

    private sealed record MyPupilsTestContext(string MyPupilsId, List<string> MyPupilUpns);

    [Fact]
    public async Task DeletePupilsFromMyPupils_DeletesAPupil_When_Identifier_Is_Part_Of_MyPupils()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveApplicationType<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        string deletePupilIdentifier = _testContext!.MyPupilUpns[0];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext!.MyPupilsId,
            DeletePupilUpns: [deletePupilIdentifier]);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<MyPupilsDocumentDto>(containerName: MyPupilsContainerName));

        List<string> remainingUpnsAfterDelete =
            _testContext.MyPupilUpns.Except([deletePupilIdentifier]).ToList();

        MyPupilsDocumentDto myPupilsDocumentDto = Assert.Single(users);
        Assert.NotNull(myPupilsDocumentDto);
        Assert.Equivalent(remainingUpnsAfterDelete, myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_DeletesManyPupils_When_Many_Identifiers_Are_Part_Of_MyPupils()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveApplicationType<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        List<string> myPupilUpns = _testContext!.MyPupilUpns;

        List<string> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            myPupilUpns[4],
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId,
            DeletePupilUpns: deleteMultiplePupilIdentifiers);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<MyPupilsDocumentDto>(MyPupilsContainerName));

        List<string> remainingUpnsAfterDelete =
            myPupilUpns.Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn)).ToList();

        MyPupilsDocumentDto myPupilsDocument = Assert.Single(users);
        Assert.NotNull(myPupilsDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, myPupilsDocument.MyPupils.Pupils.Select(t => t.UPN));
    }


    [Fact]
    public async Task DeletePupilsFromMyPupils_DeletesPupilsInList_When_Some_Identifiers_Are_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveApplicationType<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        List<string> deleteMultiplePupilIdentifiers =
        [
            _testContext!.MyPupilUpns[0],
            UniquePupilNumberTestDoubles.Generate(1).Single().Value, // Unknown identifier not part of the list
            null!, // Null identifier
            _testContext!.MyPupilUpns[_testContext!.MyPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId,
            deleteMultiplePupilIdentifiers);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> myPupilsDocument =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<MyPupilsDocumentDto>(containerName: MyPupilsContainerName));

        List<string> remainingUpnsAfterDelete =
            _testContext.MyPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .ToList();

        MyPupilsDocumentDto actualDocument = Assert.Single(myPupilsDocument);
        Assert.NotNull(actualDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, actualDocument.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Matches_NoPupilsInList_Throws_Does_Not_Update_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveApplicationType<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        List<string> deletePupilIdentifiersNotInList =
            UniquePupilNumberTestDoubles.Generate(2)
                .Select(t => t.Value)
                    .ToList();

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext!.MyPupilsId,
            deletePupilIdentifiersNotInList);

        // Act
        Func<Task> act = () => sut.HandleRequestAsync(request);
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal($"None of the DeletePupilUpns are part of User: {_testContext.MyPupilsId} MyPupils", ex.Message);

        // Assert
        IEnumerable<MyPupilsDocumentDto> myPupilsDocument =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<MyPupilsDocumentDto>(containerName: MyPupilsContainerName));

        List<string> remainingUpnsAfterDelete =
            _testContext.MyPupilUpns
                .Where((upn) => !deletePupilIdentifiersNotInList.Contains(upn))
                .ToList();

        MyPupilsDocumentDto actualDocument = Assert.Single(myPupilsDocument);
        Assert.NotNull(actualDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, _testContext.MyPupilUpns);
    }
}
