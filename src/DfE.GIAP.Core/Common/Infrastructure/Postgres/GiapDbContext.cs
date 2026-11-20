using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres;

public class GiapDbContext(DbContextOptions<GiapDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public const string ConnectionName = "GiapDb";

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<LocalAuthority> LocalAuthorities => Set<LocalAuthority>();

    public DbSet<Establishment> Establishments => Set<Establishment>();

    public DbSet<EstablishmentGroup> EstablishmentGroups => Set<EstablishmentGroup>();

    public DbSet<EstablishmentGroupLink> EstablishmentGroupLinks => Set<EstablishmentGroupLink>();

    public DbSet<NpdPupil> NpdPupils => Set<NpdPupil>();

    public DbSet<NpdPupilRecord> NpdPupilRecords => Set<NpdPupilRecord>();

    public DbSet<PupilPremiumPupil> PupilPremiumPupils => Set<PupilPremiumPupil>();

    public DbSet<FurtherEducationLearner> FurtherEducationLearners => Set<FurtherEducationLearner>();

    public DbSet<FurtherEducationLearnerRecord> FurtherEducationLearnerRecords => Set<FurtherEducationLearnerRecord>();

    public DbSet<IngestFile> IngestFiles => Set<IngestFile>();

    public DbSet<IngestRun> IngestRuns => Set<IngestRun>();

    public DbSet<IngestRejectedRow> IngestRejectedRows => Set<IngestRejectedRow>();

    public static GiapDbContext Create(string? connectionString, int? commandTimeout = null) => new(CreateOptions(connectionString, commandTimeout));

    public static void ConfigureOptions(DbContextOptionsBuilder builder, string? connectionString = null, int? commandTimeout = null) =>
        builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .UseSeeding((context, _) => ((GiapDbContext)context).SeedData())
            .UseAsyncSeeding((context, _, cancellationToken) => ((GiapDbContext)context).SeedDataAsync(cancellationToken));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiapDbContext).Assembly);
    }

#pragma warning disable CA1822  // Method can be static
    private void SeedData()
    {
        // Add seed data here; ensure that both SeedData() and SeedDataAsync() are implemented
        // https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
    }

    private async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        // Add seed data here; ensure that both SeedData() and SeedDataAsync() are implemented
        // https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
    }
#pragma warning restore CA1822

    private static DbContextOptions<GiapDbContext> CreateOptions(string? connectionString, int? commandTimeout)
    {
        DbContextOptionsBuilder<GiapDbContext> optionsBuilder = new DbContextOptionsBuilder<GiapDbContext>();
        ConfigureOptions(optionsBuilder, connectionString, commandTimeout);
        return optionsBuilder.Options;
    }
}
