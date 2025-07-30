using DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.IntegrationTests.MyPupils.Extensions;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
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
        using AzureSearchMockFixture mockSearchFixture = new(
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>().Value);

        IEnumerable<AzureIndexEntity> npdSearchindexDtos = mockSearchFixture.StubNpd();
        IEnumerable<AzureIndexEntity> pupilPremiumSearchIndexDtos = mockSearchFixture.StubPupilPremium();

        UserId userId = new(Guid.NewGuid().ToString());

        IEnumerable<UniquePupilNumber> upns = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).Select(t => t.UPN).ToUniquePupilNumbers();

        await CosmosDbFixture.Database.WriteItemAsync<UserDto>(
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

        List<PupilDto> expectedPupils = npdSearchindexDtos.Concat(pupilPremiumSearchIndexDtos).MapToPupilDto().ToList();

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
}

