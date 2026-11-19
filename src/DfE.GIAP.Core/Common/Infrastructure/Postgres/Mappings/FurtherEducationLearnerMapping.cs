using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class FurtherEducationLearnerMapping : IEntityTypeConfiguration<FurtherEducationLearner>
{
    public void Configure(EntityTypeBuilder<FurtherEducationLearner> builder)
    {
        builder.ToTable("further_education_learners");
        builder.HasKey(l => l.Uln);
        builder.HasTrigramIndex(l => l.Surname);
        builder.HasTrigramIndex(l => l.Forename);
        builder.HasIndex(l => l.Dob);
    }
}
