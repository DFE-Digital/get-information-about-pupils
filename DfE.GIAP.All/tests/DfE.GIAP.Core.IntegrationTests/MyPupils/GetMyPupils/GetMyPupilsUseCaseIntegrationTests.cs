using DfE.GIAP.Core.IntegrationTests.Fixture.AzureSearch;
using DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.GetMyPupils;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetMyPupilsUseCaseIntegrationTests : BaseIntegrationTest
{
    public GetMyPupilsUseCaseIntegrationTests(CosmosDbFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void MyTest()
    {
        SearchIndexOptions options =
            ResolveTypeFromScopedContext<IOptions<SearchIndexOptions>>().Value;

        using AzureSearchMockFixture mockSearchFixture = new(options);

        // TODO write a User document with some UPNs - Fixture.Database.
        // Generate a AzureIndexEntity to stub
        
        mockSearchFixture.StubSearchResponse(
            indexName: options.GetIndexOptionsByName("pupil-premium").IndexName, null!);
        mockSearchFixture.StubSearchResponse(
            indexName: options.GetIndexOptionsByName("npd").IndexName, null!);

        Assert.True(true);
    }
}

