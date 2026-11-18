using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class NpdPupilRecordMapping : IEntityTypeConfiguration<NpdPupilRecord>
{
    public void Configure(EntityTypeBuilder<NpdPupilRecord> builder)
    {
        builder.ToTable("npd_pupil_records", t => t.HasCheckConstraint(
            "ck_npd_pupil_records_ingest_version",
            $"ingest_version BETWEEN {(short)IngestVersion.Unamended} AND {(short)IngestVersion.Final}"));
        builder.HasKey(r => new { r.PupilMatchingRef, r.Dataset, r.AcademicYear });
        builder.Property(r => r.Dataset).HasConversion<string>();
        builder.Property(r => r.Data).HasColumnType("jsonb");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.HasOne<NpdPupil>().WithMany().HasForeignKey(r => r.PupilMatchingRef);
        // A file replaces all the records for its dataset and academic year
        builder.HasIndex(r => new { r.Dataset, r.AcademicYear });
    }
}
