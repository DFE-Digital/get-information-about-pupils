using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionMarker : ICollectionFixture<CosmosDbFixture>, ICollectionFixture<SearchIndexFixture>
{
    public const string Name = "IntegrationTests";
}
