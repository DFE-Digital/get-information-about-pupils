using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class FurtherEducationLearnerRecordMapping : IEntityTypeConfiguration<FurtherEducationLearnerRecord>
{
    public void Configure(EntityTypeBuilder<FurtherEducationLearnerRecord> builder)
    {
        builder.ToTable("further_education_learner_records");
        builder.HasKey(r => new { r.Uln, r.Dataset, r.AcademicYear });
        builder.Property(r => r.Dataset).HasConversion<string>();
        builder.Property(r => r.Data).HasColumnType("jsonb");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.HasOne<FurtherEducationLearner>().WithMany().HasForeignKey(r => r.Uln);
    }
}
