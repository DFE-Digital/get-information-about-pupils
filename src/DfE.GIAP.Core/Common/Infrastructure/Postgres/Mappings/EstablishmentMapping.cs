using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class EstablishmentMapping : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.ToTable("establishments");
        builder.HasKey(e => e.Urn);
        builder.Property(e => e.Urn).ValueGeneratedNever();
        builder.HasOne<LocalAuthority>().WithMany().HasForeignKey(e => e.LaCode);
        // Not unique; a closed establishment and its successor can share an LA code and establishment number
        builder.HasIndex(e => new { e.LaCode, e.EstablishmentNumber });
    }
}
