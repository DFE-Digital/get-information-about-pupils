using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class IngestRejectedRowMapping : IEntityTypeConfiguration<IngestRejectedRow>
{
    public void Configure(EntityTypeBuilder<IngestRejectedRow> builder)
    {
        builder.ToTable("ingest_rejected_rows");
        builder.HasKey(r => new { r.RunId, r.RowNumber });
        builder.Property(r => r.Data).HasColumnType("jsonb");
        builder.HasOne<IngestRun>().WithMany().HasForeignKey(r => r.RunId);
    }
}
