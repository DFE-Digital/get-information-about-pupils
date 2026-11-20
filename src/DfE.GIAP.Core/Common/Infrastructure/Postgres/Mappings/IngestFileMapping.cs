using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class IngestFileMapping : IEntityTypeConfiguration<IngestFile>
{
    public void Configure(EntityTypeBuilder<IngestFile> builder)
    {
        builder.ToTable("ingest_files");
        builder.HasKey(f => f.Path);
        builder.Property(f => f.Status).HasConversion<string>();
        builder.HasIndex(f => f.Status);
    }
}
