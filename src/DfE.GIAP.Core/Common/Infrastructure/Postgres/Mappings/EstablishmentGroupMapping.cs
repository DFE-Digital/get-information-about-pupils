using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal sealed class EstablishmentGroupMapping : IEntityTypeConfiguration<EstablishmentGroup>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroup> builder)
    {
        builder.ToTable("establishment_groups");
        builder.HasKey(g => g.GroupUid);
        builder.Property(g => g.GroupUid).ValueGeneratedNever();
    }
}
