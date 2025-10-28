using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    private readonly CosmosDbFixture _cosmosDbFixture;

    private MyPupilsTestContext? _testContext;
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture)
    {
        _cosmosDbFixture = cosmosDbFixture;
    }

    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.Database.ClearDatabaseAsync();

        services.AddMyPupilsDependencies();

        // Initialise fixture and pupils, store in context
        List<AzureIndexEntity> npdSearchindexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);
        List<AzureIndexEntity> pupilPremiumIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);

        UniquePupilNumbers myPupilsUpns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: npdSearchindexDtos.Concat(pupilPremiumIndexDtos).Select(t => t.UPN).ToUniquePupilNumbers());

        UserId userId = UserIdTestDoubles.Default();

        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(userId, myPupilsUpns);

        await _cosmosDbFixture.Database.WriteItemAsync(myPupilsDocument);

        _testContext = new MyPupilsTestContext(myPupilsUpns, userId);
        services.AddMyPupilsDependencies();
    }

    private sealed record MyPupilsTestContext(UniquePupilNumbers MyPupilUpns, UserId userId);

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
            _testContext.userId.Value,
            DeletePupilUpns: [deletePupilIdentifier],
            DeleteAll: false);

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

        List<UniquePupilNumber> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
                myPupilUpns[4],
                myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.userId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns.Where((upn) => !deleteMultiplePupilIdentifiers.Select(t => t.Value).Contains(upn.Value))
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
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext!.MyPupilUpns.GetUniquePupilNumbers();

        List<UniquePupilNumber> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            null!, // Unknown identifier not part of the list
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.userId.Value,
            deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            [.. myPupilUpns
                .Where((upn) => !request.DeletePupilUpns.Contains(upn))
                .Select(t => t.Value)];

        MyPupilsDocumentDto actualUserDto = Assert.Single(users);
        Assert.NotNull(actualUserDto);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, actualUserDto.MyPupils.Pupils.Select(t => t.UPN));
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
