using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Mappings;

internal static class MappingExtensions
{
    // A trigram index supports fuzzy and partial name matching (similarity, LIKE and ILIKE)
    public static IndexBuilder<T> HasTrigramIndex<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, object?>> property)
        where T : class =>
        builder.HasIndex(property).HasMethod("gin").HasOperators("gin_trgm_ops");
}
