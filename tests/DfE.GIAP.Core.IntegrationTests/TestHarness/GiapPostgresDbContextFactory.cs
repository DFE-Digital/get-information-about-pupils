using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

public sealed class GiapPostgresDbContextFactory(GiapPostgresFixture postgres) : IDbContextFactory<GiapDbContext>
{
    public GiapDbContext CreateDbContext() => postgres.CreateDbContext();
}
