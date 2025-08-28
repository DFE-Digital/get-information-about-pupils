using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.Fixture.SearchIndex;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.UseCases;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    public GetMyPupilsUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture)
    {
    }

    protected override Task OnInitializeAsync(IServiceCollection services)
    {
        services
            .AddMyPupilsDependencies()
            .ConfigureAzureSearchClients();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetMyPupils_HasPupils_In_MyPupils_Returns_Npd_And_PupilPremium_Pupils()
    {
        // Arrange
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 10);
        mockSearchFixture.StubNpdSearchIndex(npdSearchIndexDtos);

        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = AzureIndexEntityDtosTestDoubles.Generate(count: 25);
        mockSearchFixture.StubPupilPremiumSearchIndex(pupilPremiumSearchIndexDtos);

        UserId userId = UserIdTestDoubles.Default();

        IEnumerable<UniquePupilNumber> upns
            = npdSearchIndexDtos.Concat(pupilPremiumSearchIndexDtos)
                .Select((t) => t.UPN)
                    .ToUniquePupilNumbers();

        await Fixture.Database.WriteItemAsync<UserDto>(
            UserDtoTestDoubles.WithPupils(
                userId,
                upns));

        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId.Value));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.PupilDtos);
        Assert.Equal(npdSearchIndexDtos.Count() + pupilPremiumSearchIndexDtos.Count(), getMyPupilsResponse.PupilDtos.Count);

        MapAzureSearchIndexDtosToPupilDtos mapAzureSearchIndexDtosToPupilDtosMapper = new();
        List<PupilDto> expectedPupils = npdSearchIndexDtos.Select(mapAzureSearchIndexDtosToPupilDtosMapper.Map).ToList();

        foreach (PupilDto expectedPupil in expectedPupils)
        {
            PupilDto? actual = getMyPupilsResponse.PupilDtos.Pupils.Single(pupil => pupil.UniquePupilNumber == expectedPupil.UniquePupilNumber);

            Assert.NotNull(actual);
            Assert.Equal(expectedPupil.Forename, actual.Forename);
            Assert.Equal(expectedPupil.Surname, actual.Surname);
            Assert.Equal(expectedPupil.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expectedPupil.Sex, actual.Sex);
            Assert.Equal(expectedPupil.LocalAuthorityCode, actual.LocalAuthorityCode);
            Assert.Equal(expectedPupil.LocalAuthorityCode, actual.LocalAuthorityCode);

            bool isPupilPremium = pupilPremiumSearchIndexDtos.Any(t => t.UPN == expectedPupil.UniquePupilNumber);
            Assert.Equal(isPupilPremium, actual!.IsPupilPremium);
        }
    }

    [Fact]
    public async Task GetMyPupils_NoPupils_Returns_Empty_And_DoesNot_Call_SearchIndexes()
    {
        // Arrange
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        UserId userId = UserIdTestDoubles.Default();

        await Fixture.Database.WriteItemAsync<UserDto>(
            UserDtoTestDoubles.WithPupils(
                userId,
                upns: []));
        // Act
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> sut =
            ResolveTypeFromScopedContext<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>();

        GetMyPupilsResponse getMyPupilsResponse =
            await sut.HandleRequestAsync(
                new GetMyPupilsRequest(userId.Value));

        // Assert
        Assert.NotNull(getMyPupilsResponse);
        Assert.NotNull(getMyPupilsResponse.PupilDtos);

        Assert.Equivalent(PupilDtos.Empty(), getMyPupilsResponse.PupilDtos);
    }

    private sealed class MapAzureSearchIndexDtosToPupilDtos : IMapper<AzureIndexEntity, PupilDto>
    {
        public PupilDto Map(AzureIndexEntity input)
        {
            return new()
            {
                UniquePupilNumber = input.UPN,
                DateOfBirth = input.DOB?.ToString("yyyy-MM-dd") ?? string.Empty,
                Forename = input.Forename,
                Surname = input.Surname,
                Sex = input.Sex?.ToString() ?? string.Empty,
                IsPupilPremium = false, // not used when asserting - not mapped from entity, rather that the pupil-premium index was called.
                LocalAuthorityCode = int.Parse(input.LocalAuthority),
            };
        }
    }
}
