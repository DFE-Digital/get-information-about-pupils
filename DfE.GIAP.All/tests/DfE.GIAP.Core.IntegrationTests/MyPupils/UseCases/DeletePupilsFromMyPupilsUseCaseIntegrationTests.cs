using DfE.GIAP.Core.IntegrationTests.Fixture.Configuration;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest, IClassFixture<ConfigurationFixture>
{
    private readonly ConfigurationFixture _configFixture;
    private readonly CosmosDbFixture _cosmosDbFixture;
    private SearchIndexFixture? _mockSearchFixture;
    private MyPupilsTestContext? _testContext;

    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(
        CosmosDbFixture cosmosDbFixture, ConfigurationFixture configurationFixture)
    {
        _cosmosDbFixture = cosmosDbFixture;
        _configFixture = configurationFixture;
        _mockSearchFixture = null;
        _testContext = null;
    }

    private sealed record MyPupilsTestContext(UniquePupilNumbers MyPupilUpns, UserId UserId);
    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        await _cosmosDbFixture.Database.ClearDatabaseAsync();

        _mockSearchFixture = new SearchIndexFixture();

        services
            .AddSharedTestDependencies(
                SearchIndexOptionsStub.StubFor(_mockSearchFixture.BaseUrl))
            .AddMyPupilsDependencies()
            .AddSearchDependencies(_configFixture.Configuration);

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = _mockSearchFixture.StubNpdSearchIndex();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = _mockSearchFixture.StubPupilPremiumSearchIndex();

        List<AzureIndexEntity> myPupils = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).ToList();

        UniquePupilNumbers myPupilsUpns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: myPupils.Select(t => t.UPN).ToUniquePupilNumbers());

        UserId userId = UserIdTestDoubles.Default();
        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(userId, myPupilsUpns);
        await _cosmosDbFixture.Database.WriteItemAsync(myPupilsDocument);
        _testContext = new MyPupilsTestContext(myPupilsUpns, userId);
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        UniquePupilNumber deletePupilIdentifier = _testContext!.MyPupilUpns.GetUniquePupilNumbers()[0];

        DeletePupilsFromMyPupilsRequest request = new DeletePupilsFromMyPupilsRequest(_testContext.UserId.Value,
            DeletePupilUpns: [deletePupilIdentifier], DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>();
        List<string> remainingUpnsAfterDelete =
            _testContext.MyPupilUpns.GetUniquePupilNumbers()
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

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext!.MyPupilUpns.GetUniquePupilNumbers();

        List<UniquePupilNumber> deleteMultiplePupilIdentifiers =
        [
            myPupilUpns[0],
            myPupilUpns[4],
            myPupilUpns[myPupilUpns.Count - 1]
        ];

        DeletePupilsFromMyPupilsRequest request = new DeletePupilsFromMyPupilsRequest(_testContext.UserId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers, DeleteAll: false);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            [.. myPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn))
                .Select(t => t.Value)];

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

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext!.MyPupilUpns.GetUniquePupilNumbers();


        DeletePupilsFromMyPupilsRequest request = new DeletePupilsFromMyPupilsRequest(UserId: _testContext.UserId.Value,
            DeletePupilUpns:
            [
                myPupilUpns[0],
                null!, // Unknown identifier not part of the list
                myPupilUpns[myPupilUpns.Count - 1]
            ], DeleteAll: false);

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

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_All_Items_When_DeleteAll_Is_True()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        const bool deleteAllPupils = true;

        DeletePupilsFromMyPupilsRequest request = new DeletePupilsFromMyPupilsRequest(_testContext!.UserId.Value,
            DeletePupilUpns: [null!], // should be ignored if DeleteAll is enabled
            DeleteAll: deleteAllPupils);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        MyPupilsDocumentDto myPupilsDocument = Assert.Single(await _cosmosDbFixture.Database.ReadManyAsync<MyPupilsDocumentDto>());
        Assert.NotNull(myPupilsDocument);
        Assert.Equal(_testContext.UserId.Value, myPupilsDocument.id);
        Assert.Empty(myPupilsDocument.MyPupils.Pupils);
    }

    protected override Task OnDisposeAsync()
    {
        _mockSearchFixture?.Dispose();
        return Task.CompletedTask;
    }
}
