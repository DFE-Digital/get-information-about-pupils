using DfE.GIAP.SharedTests.Fixtures.CosmosDb;

namespace DfE.GIAP.Core.IntegrationTests;
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionMarker : ICollectionFixture<CosmosDbFixture>
{
    public const string Name = "IntegrationTests";
}
