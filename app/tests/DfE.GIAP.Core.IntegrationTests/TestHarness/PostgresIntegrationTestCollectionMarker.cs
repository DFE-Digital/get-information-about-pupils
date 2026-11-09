using DfE.GIAP.SharedTests.Postgres;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

/// <summary>
/// Shares one migrated PostgreSQL container between the tests that need it.
/// <para>
/// Only apply this to test classes that genuinely need the database. Tests that stub their
/// dependencies in-process should stay out of the collection so they can run in parallel without
/// any infrastructure.
/// </para>
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresIntegrationTestCollectionMarker : ICollectionFixture<GiapPostgresFixture>
{
    public const string Name = "PostgresIntegrationTests";
}
