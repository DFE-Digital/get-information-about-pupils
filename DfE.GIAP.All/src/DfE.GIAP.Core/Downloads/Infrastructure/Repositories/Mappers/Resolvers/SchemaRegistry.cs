using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

public static class SchemaRegistry
{
    private static readonly Dictionary<Type, List<FieldMappingDefinition>> Schemas = new()
    {
        { typeof(MtcEntry), MtcInputSchema.Fields },
        // etc...
    };

    public static List<FieldMappingDefinition>? GetSchemaFor(Type type)
        => Schemas.TryGetValue(type, out List<FieldMappingDefinition>? schema) ? schema : null;
}
