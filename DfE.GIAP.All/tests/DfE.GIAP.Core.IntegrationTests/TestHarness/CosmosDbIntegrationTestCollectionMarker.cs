namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

/// <summary>
/// Serialises the tests that share the Cosmos DB emulator, which each clear the database on setup.
/// <para>
/// Only apply this to test classes that genuinely need the emulator. Tests that stub their
/// dependencies in-process should stay out of the collection so they can run in parallel without
/// any infrastructure.
/// </para>
/// </summary>
[CollectionDefinition(Name)]
public sealed class CosmosDbIntegrationTestCollectionMarker : ICollectionFixture<GiapCosmosDbFixture>
{
    public const string Name = "CosmosDbIntegrationTests";
}
