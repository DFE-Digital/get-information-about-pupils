using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Fixtures.CosmosDb;
using DfE.GIAP.SharedTests.Fixtures.SearchIndex;
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
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base(cosmosDbFixture)
    {

    }

    private sealed record MyPupilsTestContext(UniquePupilNumbers MyPupilUpns, MyPupilsId MyPupilsId);
    protected override async Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddMyPupilsDependencies();

        // Initialise fixture and pupils, store in context
        List<AzureIndexEntity> npdSearchindexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);
        List<AzureIndexEntity> pupilPremiumIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);

        List<AzureIndexEntity> myPupils = npdSearchindexDtos.Concat(pupilPremiumIndexDtos).ToList();

        UniquePupilNumbers myPupilsUpns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: myPupils.Select(t => t.UPN).ToUniquePupilNumbers());

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();
        MyPupilsDocumentDto myPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(myPupilsId, myPupilsUpns);

        await Fixture.Database.WriteItemAsync(myPupilsDocument);

        _testContext = new MyPupilsTestContext(myPupilsUpns, myPupilsId);
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        string deletePupilIdentifier = _testContext.MyPupilUpns.GetUniquePupilNumbers()[0].Value;

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
            myPupilUpns[0].Value,
            myPupilUpns[4].Value,
            myPupilUpns[myPupilUpns.Count - 1].Value
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _testContext.MyPupilsId.Value,
            DeletePupilUpns: deleteMultiplePupilIdentifiers);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns.Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn.Value))
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

        IReadOnlyList<UniquePupilNumber> myPupilUpns = _testContext.MyPupilUpns.GetUniquePupilNumbers();

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
        IEnumerable<MyPupilsDocumentDto> users = await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>();

        List<string> remainingUpnsAfterDelete =
            myPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn.Value))
                .Select(t => t.Value)
                .ToList();

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
        MyPupilsDocumentDto myPupilsDocument = Assert.Single(await Fixture.Database.ReadManyAsync<MyPupilsDocumentDto>());
        Assert.NotNull(myPupilsDocument);
        Assert.Equal(_testContext.UserId.Value, myPupilsDocument.id);
        Assert.Empty(myPupilsDocument.MyPupils.Pupils);
    }
     */
}
