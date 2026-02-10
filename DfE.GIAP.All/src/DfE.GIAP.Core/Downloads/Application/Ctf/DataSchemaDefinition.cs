namespace DfE.GIAP.Core.Downloads.Application.Ctf;

public class DataSchemaDefinition
{
    public string? Year { get; set; }
    public List<DataSchemaDefinitionRule>? Rules { get; set; }
}

public class DataSchemaDefinitionRule
{
    public string Stage { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string ResultQualifier { get; set; } = string.Empty;
    public string ResultField { get; set; } = string.Empty;
}
