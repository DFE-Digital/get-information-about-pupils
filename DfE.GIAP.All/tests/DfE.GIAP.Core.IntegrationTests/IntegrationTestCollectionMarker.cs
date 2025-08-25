using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

namespace DfE.GIAP.Core.IntegrationTests;
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionMarker : ICollectionFixture<CosmosDbFixture>
{
    public const string Name = "CoreIntegrationTests";
}
