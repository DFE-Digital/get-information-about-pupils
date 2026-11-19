using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class EstablishmentGroupLinkMapping : IEntityTypeConfiguration<EstablishmentGroupLink>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroupLink> builder)
    {
        builder.ToTable("establishment_group_links");
        builder.HasKey(l => new { l.GroupUid, l.Urn });
        builder.HasOne<EstablishmentGroup>().WithMany().HasForeignKey(l => l.GroupUid);
        builder.HasIndex(l => l.Urn);
    }
}
