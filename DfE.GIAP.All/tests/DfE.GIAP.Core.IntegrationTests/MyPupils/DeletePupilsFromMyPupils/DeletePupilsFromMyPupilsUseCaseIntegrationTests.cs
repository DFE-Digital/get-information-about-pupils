using DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.MyPupils.Extensions;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.SharedTests.TestDoubles.Users;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.DeletePupilsFromMyPupils;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base(cosmosDbFixture)
    {

    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddMyPupilsDependencies();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Throws_InvalidArgumentException_When_Any_PupilIdentifier_Is_Not_Part_Of_The_List()
    {
        // Arrange
        using AzureSearchMockFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>().Value);

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpd();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremium();

        UserId userId = UserIdTestDoubles.Default();

        List<MyPupilItemDto> myPupils = npdSearchindexDtos.MapToMyPupilsItemDto().Concat(pupilPremiumSearchIndexDtos.MapToMyPupilsItemDto()).ToList();

        await CosmosDbFixture.Database.WriteItemAsync<UserDto>(
            UserDtoTestDoubles.WithPupils(
                userId,
                myPupils));

        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        IEnumerable<string> unknownPupilIdentifier = [Guid.NewGuid().ToString()];

        // Act
        DeletePupilsFromMyPupilsRequest request = new(
            userId.Value,
            PupilIdentifiers: unknownPupilIdentifier,
            DeleteAll: false);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sut.HandleRequestAsync(request));

        IEnumerable<UserDto> users = await CosmosDbFixture.Database.ReadManyAsync<UserDto>();
        UserDto userDto = Assert.Single(users);
        Assert.NotNull(userDto);
        Assert.Equivalent(myPupils, userDto.MyPupils.Pupils);
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        using AzureSearchMockFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>().Value);

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpd();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremium();

        UserId userId = UserIdTestDoubles.Default();

        List<MyPupilItemDto> myPupils =
            npdSearchindexDtos.MapToMyPupilsItemDto()
                .Concat(pupilPremiumSearchIndexDtos.MapToMyPupilsItemDto()).ToList();

        await CosmosDbFixture.Database.WriteItemAsync<UserDto>(
            UserDtoTestDoubles.WithPupils(
                userId,
                myPupils));

        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        string deletePupilIdentifier = myPupils[0].UPN;
        // Act
        DeletePupilsFromMyPupilsRequest request = new(
            userId.Value,
            PupilIdentifiers: [deletePupilIdentifier],
            DeleteAll: false);

        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<UserDto> users = await CosmosDbFixture.Database.ReadManyAsync<UserDto>();
        UserDto userDto = Assert.Single(users);
        Assert.NotNull(userDto);
        Assert.DoesNotContain(userDto.MyPupils.Pupils, t => t.UPN == deletePupilIdentifier);
        Assert.Equal(
            npdSearchindexDtos.Count() + pupilPremiumSearchIndexDtos.Count() - 1,
            userDto.MyPupils.Pupils.Count());
    }
}
