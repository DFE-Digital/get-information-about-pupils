namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

public class FieldMappingDefinition
{
    /// <summary>
    /// Gets or sets the collection of all possible JSON source names associated with this instance.
    /// </summary>
    public List<string> SourceNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the property on the target domain model.
    /// </summary>
    public string TargetProperty { get; set; } = default!;

    /// <summary>
    /// Gets or sets the target type to which the value should be converted.
    /// </summary>
    public Type TargetType { get; set; } = typeof(string);
}
