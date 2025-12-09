using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly CosmosDbFixture _cosmosDbFixture;

    private MyPupilsTestContext? _testContext;
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture)
    {
        _cosmosDbFixture = cosmosDbFixture;
    }

    private sealed record MyPupilsTestContext(UniquePupilNumbers MyPupilUpns, MyPupilsId MyPupilsId);
    protected async override Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName,
            (client) => client.ClearDatabaseAsync());

        services.AddMyPupilsCore();

        // Initialise fixture and pupils, store in context
        List<AzureNpdSearchResponseDto> npdSearchindexDtos = AzureNpdSearchResponseDtoTestDoubles.Generate(count: 10);
        List<AzureNpdSearchResponseDto> pupilPremiumIndexDtos = AzureNpdSearchResponseDtoTestDoubles.Generate(count: 10);

        
        UniquePupilNumbers myPupilsUpns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers:
                    npdSearchindexDtos.Concat(pupilPremiumIndexDtos)
                        .Select(t => t.UPN)
                        .ToUniquePupilNumbers());

        MyPupilsId myPupilId = MyPupilsIdTestDoubles.Default();

        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(myPupilId, myPupilsUpns);

        await _cosmosDbFixture.InvokeAsync(
            databaseName: _cosmosDbFixture.DatabaseName, (client) => client.WriteItemAsync(containerName: "mypupils", myPupilsDocument));

        _testContext = new MyPupilsTestContext(myPupilsUpns, myPupilId);
    }


    // TODO fixed as part of MyPupils work
    /*
    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        UniquePupilNumber deletePupilIdentifier = _testContext.MyPupilUpns.GetUniquePupilNumbers()[0];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId.Value,
            DeletePupilUpns: [deletePupilIdentifier]);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete = _testContext.MyPupilUpns.GetUniquePupilNumbers()
            .Select(t => t.Value)
            .Where((upn) => !upn.Equals(deletePupilIdentifier))
            .ToList();

        MyPupilsDocumentDto myPupilsDocumentDto = Assert.Single(users);
        Assert.NotNull(myPupilsDocumentDto);
        Assert.Equivalent(remainingUpnsAfterDelete, myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Multiples_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext.MyPupilUpns.GetUniquePupilNumbers();

        List<string> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            myPupilUpns[4],
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns.Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .Select(t => t.Value).ToList();

        MyPupilsDocumentDto myPupilsDocument = Assert.Single(users);
        Assert.NotNull(myPupilsDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, myPupilsDocument.MyPupils.Pupils.Select(t => t.UPN));
    }*/

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Multiples_When_Some_PupilIdentifiers_Are_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveApplicationType<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext!.MyPupilUpns.GetUniquePupilNumbers();

        List<string> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0].Value,
            null!, // Unknown identifier not part of the list
            myPupilUpns[myPupilUpns.Count - 1].Value
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId.Value,
            deleteMultiplePupilIdentifiers);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> myPupilsDocument =
            await _cosmosDbFixture.InvokeAsync(
                databaseName: _cosmosDbFixture.DatabaseName,
                (client) => client.ReadManyAsync<MyPupilsDocumentDto>(containerName: "mypupils"));

        List<string> remainingUpnsAfterDelete =
            myPupilUpns
                .Select(t => t.Value)
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .ToList();

        MyPupilsDocumentDto actualDocument = Assert.Single(myPupilsDocument);
        Assert.NotNull(actualDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, actualDocument.MyPupils.Pupils.Select(t => t.UPN));
    }

    /* TODO for DeleteAll
     * 
     *     [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_All_Items_When_DeleteAll_Is_True()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        bool deleteAllPupils = true;
        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId,
            DeletePupilUpns: [null!], // should be ignored if DeleteAll is enabled

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        MyPupilsDocumentDto myPupilsDocument = Assert.Single(await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>());
        Assert.NotNull(myPupilsDocument);
        Assert.Equal(_testContext.UserId.Value, myPupilsDocument.id);
        Assert.Empty(myPupilsDocument.MyPupils.Pupils);
    }
     */
}
