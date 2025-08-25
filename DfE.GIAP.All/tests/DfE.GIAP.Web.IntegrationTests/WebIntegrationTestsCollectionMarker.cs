using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

namespace DfE.GIAP.Web.IntegrationTests;
[CollectionDefinition(Name)]
public sealed class WebIntegrationTestsCollectionMarker : ICollectionFixture<CosmosDbFixture>
{
    public const string Name = "WebIntegrationTests";
}
