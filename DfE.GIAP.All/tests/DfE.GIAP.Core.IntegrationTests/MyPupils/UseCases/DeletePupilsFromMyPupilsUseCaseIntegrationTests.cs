using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.Configuration;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
#nullable disable
    private MyPupilsTestContext _testContext;
#nullable enable
    private CosmosDbFixture Fixture { get; }

    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base()
    {
        Fixture = cosmosDbFixture;
    }

    private sealed record MyPupilsTestContext(SearchIndexFixture fixture, UniquePupilNumbers myPupilUpns, UserId UserId);
    protected async override Task OnInitializeAsync(IServiceCollection services)
    {
        await Fixture.Database.ClearDatabaseAsync();

        services
            .AddMyPupilsDependencies();

        // Initialise fixture and pupils, store in context
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpdSearchIndex();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremiumSearchIndex();

        List<AzureIndexEntity> myPupils = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).ToList();
        UniquePupilNumbers myPupilsUpns =
            UniquePupilNumbers.Create(uniquePupilNumbers: myPupils.Select(t => t.UPN).ToUniquePupilNumbers());

        UserId userId = UserIdTestDoubles.Default();
        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(userId, myPupilsUpns);
        await Fixture.Database.WriteItemAsync(myPupilsDocument);
        _testContext = new MyPupilsTestContext(mockSearchFixture, myPupilsUpns, userId);
    }

    protected override Task OnDisposeAsync()
    {
        _testContext?.fixture?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        UniquePupilNumber deletePupilIdentifier = _testContext.myPupilUpns.GetUniquePupilNumbers()[0];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.UserId.Value,
            DeletePupilUpns: [deletePupilIdentifier],
            DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();
        List<string> remainingUpnsAfterDelete =
            _testContext.myPupilUpns.GetUniquePupilNumbers()
                .Where((upn) => !upn.Equals(deletePupilIdentifier))
                .Select(t => t.Value).ToList();

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

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext.myPupilUpns.GetUniquePupilNumbers();

        List<UniquePupilNumber> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            myPupilUpns[4],
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.UserId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns.Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .Select(t => t.Value).ToList();

        MyPupilsDocumentDto myPupilsDocument = Assert.Single(users);
        Assert.NotNull(myPupilsDocument);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, myPupilsDocument.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Multiples_When_Some_PupilIdentifiers_Are_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext.myPupilUpns.GetUniquePupilNumbers();

        List<UniquePupilNumber> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            null, // Unknown identifier not part of the list
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.UserId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .Select(t => t.Value)
                .ToList();

        MyPupilsDocumentDto actualUserDto = Assert.Single(users);
        Assert.NotNull(actualUserDto);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, actualUserDto.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_All_Items_When_DeleteAll_Is_True()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        bool deleteAllPupils = true;
        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.UserId.Value,
            DeletePupilUpns: [null!], // should be ignored if DeleteAll is enabled
            DeleteAll: deleteAllPupils);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        MyPupilsDocumentDto myPupilsDocument = Assert.Single(await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>());
        Assert.NotNull(myPupilsDocument);
        Assert.Equal(_testContext.UserId.Value, myPupilsDocument.id);
        Assert.Empty(myPupilsDocument.MyPupils.Pupils);
    }
}
