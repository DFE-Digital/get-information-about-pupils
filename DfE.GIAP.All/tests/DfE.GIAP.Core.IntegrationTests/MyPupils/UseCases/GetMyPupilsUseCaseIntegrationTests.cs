using System.Net;
using Azure.Search.Documents;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public async Task GetMyPupils_HasSomePupilsInList_Returns_Pupils()
    {
        // Arrange
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpdSearchIndex();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremiumSearchIndex();

        UserId userId = new(Guid.NewGuid().ToString());

        IEnumerable<UniquePupilNumber> upns = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).Select(t => t.UPN).ToUniquePupilNumbers();

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
        Assert.NotNull(getMyPupilsResponse.Pupils);

        MapAzureSearchIndexDtosToPupilDtos mapAzureSearchIndexDtosToPupilDtosMapper = new();
        List<PupilDto> expectedPupils =
            npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos)
                .Select(mapAzureSearchIndexDtosToPupilDtosMapper.Map).ToList();

        Assert.Equal(expectedPupils.Count, getMyPupilsResponse.Pupils.Count());

        foreach (PupilDto expected in expectedPupils)
        {
            PupilDto? actual = getMyPupilsResponse.Pupils.SingleOrDefault(p => p.UniquePupilNumber == expected.UniquePupilNumber);

            Assert.NotNull(actual);
            Assert.Equal(expected.Forename, actual.Forename);
            Assert.Equal(expected.Surname, actual.Surname);
            Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expected.Sex, actual.Sex);
            Assert.Equal(expected.LocalAuthorityCode, actual.LocalAuthorityCode);
            Assert.Equal(expected.LocalAuthorityCode, actual.LocalAuthorityCode);

            bool isPupilPremium = pupilPremiumSearchIndexDtos.Any(t => t.UPN == expected.UniquePupilNumber);
            Assert.Equal(isPupilPremium, actual.IsPupilPremium);
        }
    }

    [Fact]
    public async Task GetMyPupils_Requests_NoPupils_Returns_NoPupils()
    {
        // Arrange
        using SearchIndexFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>());

        UserId userId = new(Guid.NewGuid().ToString());

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
        Assert.NotNull(getMyPupilsResponse.Pupils);

        Assert.Equivalent(Array.Empty<PupilDto>(), getMyPupilsResponse.Pupils);
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
