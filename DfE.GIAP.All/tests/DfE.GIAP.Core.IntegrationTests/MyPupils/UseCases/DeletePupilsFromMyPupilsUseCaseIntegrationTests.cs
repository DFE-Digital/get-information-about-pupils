using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;

[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class DeletePupilsFromMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
#nullable disable
    private MyPupilsTestContext _context;
#nullable enable
    public DeletePupilsFromMyPupilsUseCaseIntegrationTests(CosmosDbFixture cosmosDbFixture) : base(cosmosDbFixture)
    {

    }

    private sealed record MyPupilsTestContext(SearchIndexFixture fixture, List<UniquePupilNumber> myPupilUpns, UserDto user);
    protected async override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddMyPupilsDependencies();

        // Initialise fixture and pupils, store in context
        SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpdSearchIndex();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremiumSearchIndex();

        List<AzureIndexEntity> myPupils = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).ToList();
        List<UniquePupilNumber> myPupilsUpns = myPupils.Select(t => t.UPN).ToUniquePupilNumbers().ToList();

        UserId userId = UserIdTestDoubles.Default();
        UserDto seededUserDto = UserDtoTestDoubles.WithPupils(userId, myPupilsUpns);
        await Fixture.Database.WriteItemAsync(seededUserDto);
        _context = new MyPupilsTestContext(mockSearchFixture, myPupilsUpns, seededUserDto);
    }

    protected override Task OnDisposeAsync()
    {
        _context?.fixture?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Item_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        // Act
        string deletePupilIdentifier = _context.myPupilUpns[0].Value;
        DeletePupilsFromMyPupilsRequest request = new(
            _context.user.id,
            DeletePupilUpns: [deletePupilIdentifier],
            DeleteAll: false);

        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<UserDto> users = await Fixture.Database.ReadManyAsync<UserDto>();
        List<string> remainingUpnsAfterDelete =
            _context.myPupilUpns
                .Where((upn) => upn.Value != deletePupilIdentifier)
                .Select(t => t.Value).ToList();

        UserDto actualUserDTO = Assert.Single(users);
        Assert.NotNull(actualUserDTO);
        Assert.Equivalent(remainingUpnsAfterDelete, actualUserDTO.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Multiples_When_PupilIdentifier_Is_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        // Act
        List<string> deleteMultiplePupilIdentifiers =
        [
            _context.myPupilUpns[0].Value,
            _context.myPupilUpns[4].Value,
            _context.myPupilUpns[_context.myPupilUpns.Count - 1].Value
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _context.user.id,
            DeletePupilUpns: deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<UserDto> users = await Fixture.Database.ReadManyAsync<UserDto>();

        List<string> remainingUpnsAfterDelete =
            _context.myPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn.Value))
                .Select(t => t.Value).ToList();

        UserDto actualUserDto = Assert.Single(users);
        Assert.NotNull(actualUserDto);
        Assert.NotEmpty(remainingUpnsAfterDelete);
        Assert.Equivalent(remainingUpnsAfterDelete, actualUserDto.MyPupils.Pupils.Select(t => t.UPN));
    }

    [Fact]
    public async Task DeletePupilsFromMyPupils_Deletes_Multiples_When_Some_PupilIdentifiers_Are_Part_Of_The_List()
    {
        // Arrange
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> sut =
            ResolveTypeFromScopedContext<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>();

        // Act
        List<string> deleteMultiplePupilIdentifiers =
        [
            _context.myPupilUpns[0].Value,
            Guid.NewGuid().ToString(), // Unknown identifier not part of the list
            _context.myPupilUpns[_context.myPupilUpns.Count - 1].Value
        ];

        DeletePupilsFromMyPupilsRequest request = new(
            _context.user.id,
            DeletePupilUpns: deleteMultiplePupilIdentifiers,
            DeleteAll: false);

        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<UserDto> users = await Fixture.Database.ReadManyAsync<UserDto>();

        List<string> remainingUpnsAfterDelete =
            _context.myPupilUpns
                .Where((upn) => !deleteMultiplePupilIdentifiers.Contains(upn.Value))
                .Select(t => t.Value).ToList();

        UserDto actualUserDto = Assert.Single(users);
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

        // Act
        bool deleteAllPupils = true;
        DeletePupilsFromMyPupilsRequest request = new(
            _context.user.id,
            DeletePupilUpns: [Guid.NewGuid().ToString()], // should be ignored if DeleteAll is enabled
            DeleteAll: deleteAllPupils);

        await sut.HandleRequestAsync(request);

        // Assert
        IEnumerable<UserDto> users = await Fixture.Database.ReadManyAsync<UserDto>();
        UserDto userDto = Assert.Single(users);
        Assert.NotNull(userDto);
        Assert.Equal(_context.user.id, userDto.id);
        Assert.Empty(userDto.MyPupils.Pupils);
    }
}
