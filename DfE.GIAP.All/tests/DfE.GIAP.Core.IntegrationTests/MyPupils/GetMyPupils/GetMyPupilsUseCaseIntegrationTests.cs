using DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Users;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.GetMyPupils;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    public GetMyPupilsUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture)
    {
    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services.AddMyPupilsDependencies();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetMyPupils_HasSomePupilsInList_Returns_Pupils()
    {
        // Arrange
        SearchIndexOptions options = ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>().Value;

        using AzureSearchMockFixture mockSearchFixture = new(options);

        IEnumerable<AzureIndexEntity> npdSearchindexDtos
            = mockSearchFixture.StubSearchIndexResponse(
                options.GetIndexOptionsByName("npd"));

        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos =
            mockSearchFixture.StubSearchIndexResponse(
                options.GetIndexOptionsByName("pupil-premium"));

        UserId userId = new(Guid.NewGuid().ToString());

        UserDto userProfileDto = UserDtoTestDoubles.WithPupils(
            userId,
            ToMyPupilsDto(npdSearchindexDtos).Concat(ToMyPupilsDto(pupilPremiumSearchIndexDtos)));

        await CosmosDbFixture.Database.WriteItemAsync(userProfileDto);

        IAuthorisationContext authorisationContext = AuthorisationContextTestDoubles.WithUser(userId);

        // Act

        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(authorisationContext));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.Pupils);

        List<PupilDto> expectedPupils = MapSearchIndexDtoToPupilDto(
            npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos)).ToList();

        Assert.Equal(expectedPupils.Count, getMyPupilsResponse.Pupils.Count());

        foreach (PupilDto expected in expectedPupils)
        {
            PupilDto? actual = getMyPupilsResponse.Pupils.SingleOrDefault(p => p.UniquePupilNumber == expected.UniquePupilNumber);
            bool expectedToBePupilPremium = pupilPremiumSearchIndexDtos.Any(t => t.UPN == expected.UniquePupilNumber);

            Assert.NotNull(actual);
            Assert.Equal(expected.Forename, actual.Forename);
            Assert.Equal(expected.Surname, actual.Surname);
            Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expected.Sex, actual.Sex);
            Assert.Equal(expected.LocalAuthorityCode, actual.LocalAuthorityCode);
            Assert.Equal(expected.LocalAuthorityCode, actual.LocalAuthorityCode);
            Assert.Equal(expectedToBePupilPremium, actual.IsPupilPremium);
        }
    }

    private static IEnumerable<PupilDto> MapSearchIndexDtoToPupilDto(IEnumerable<AzureIndexEntity> indexDtos)
    {
        return indexDtos.Select(pupil => new PupilDto()
        {
            Id = pupil.id,
            UniquePupilNumber = pupil.UPN,
            DateOfBirth = pupil.DOB?.ToString("yyyy-MM-dd") ?? string.Empty,
            Forename = pupil.Forename,
            Surname = pupil.Surname,
            Sex = pupil.Sex?.ToString() ?? string.Empty,
            IsPupilPremium = false,
            LocalAuthorityCode = int.Parse(pupil.LocalAuthority),
        });
    }

    private static IEnumerable<MyPupilItemDto> ToMyPupilsDto(IEnumerable<AzureIndexEntity> indexDtos)
    {
        return indexDtos.Select((indexDto) => new MyPupilItemDto()
        {
            Id = Guid.NewGuid(),
            UPN = indexDto.UPN
        });
    }
}

