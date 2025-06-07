namespace DfE.GIAP.Core.IntegrationTests;
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionMarker : ICollectionFixture<CosmosDbFixture>
{
    public const string Name = "IntegrationTests";
}
