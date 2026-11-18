using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class NpdPupilMapping : IEntityTypeConfiguration<NpdPupil>
{
    public void Configure(EntityTypeBuilder<NpdPupil> builder)
    {
        builder.ToTable("npd_pupils");
        builder.HasKey(p => p.PupilMatchingRef);
        builder.HasIndex(p => p.Upn);
        builder.HasTrigramIndex(p => p.Surname);
        builder.HasTrigramIndex(p => p.Forename);
        builder.HasTrigramIndex(p => p.MiddleNames);
        builder.HasIndex(p => p.Dob);
    }
}
