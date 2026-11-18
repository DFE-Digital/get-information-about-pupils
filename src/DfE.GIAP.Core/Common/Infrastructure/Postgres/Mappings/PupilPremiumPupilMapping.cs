using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class PupilPremiumPupilMapping : IEntityTypeConfiguration<PupilPremiumPupil>
{
    public void Configure(EntityTypeBuilder<PupilPremiumPupil> builder)
    {
        builder.ToTable("pupil_premium_pupils");
        builder.HasKey(p => p.PupilMatchingRef);
        builder.Property(p => p.Data).HasColumnType("jsonb");
        builder.HasIndex(p => p.Upn);
        builder.HasTrigramIndex(p => p.Surname);
        builder.HasTrigramIndex(p => p.Forename);
        builder.HasTrigramIndex(p => p.MiddleNames);
        builder.HasIndex(p => p.Dob);
    }
}
