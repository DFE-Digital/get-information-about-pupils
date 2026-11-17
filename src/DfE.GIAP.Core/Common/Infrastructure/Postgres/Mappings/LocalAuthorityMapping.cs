using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class LocalAuthorityMapping : IEntityTypeConfiguration<LocalAuthority>
{
    public void Configure(EntityTypeBuilder<LocalAuthority> builder)
    {
        builder.ToTable("local_authorities");
        builder.HasKey(la => la.LaCode);
        builder.Property(la => la.LaCode).ValueGeneratedNever();
    }
}
