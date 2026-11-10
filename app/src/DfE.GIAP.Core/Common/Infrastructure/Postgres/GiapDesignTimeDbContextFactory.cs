using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres;

public class GiapDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GiapDbContext>
{
    public GiapDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<GiapDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        string? connectionString = configuration.GetValue<string>($"ConnectionStrings:{GiapDbContext.ConnectionName}");

        return GiapDbContext.Create(connectionString);
    }
}
