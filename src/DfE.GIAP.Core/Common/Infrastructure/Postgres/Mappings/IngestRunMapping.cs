using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class IngestRunMapping : IEntityTypeConfiguration<IngestRun>
{
    public void Configure(EntityTypeBuilder<IngestRun> builder)
    {
        builder.ToTable("ingest_runs");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Dataset).HasConversion<string>();
        builder.Property(r => r.Status).HasConversion<string>();
        builder.HasOne<IngestFile>().WithMany().HasForeignKey(r => r.FilePath);
        builder.HasIndex(r => new { r.FilePath, r.Attempt });
    }
}
