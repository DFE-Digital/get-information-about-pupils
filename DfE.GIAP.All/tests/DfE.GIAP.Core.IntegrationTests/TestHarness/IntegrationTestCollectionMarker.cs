using DfE.GIAP.SharedTests.Infrastructure.WireMock;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollectionMarker : ICollectionFixture<GiapCosmosDbFixture>, ICollectionFixture<WireMockServerFixture>
{
    public const string Name = "IntegrationTests";
}
