namespace DfE.GIAP.Core.Downloads.Application.Ctf;

public class DataMapperDefinition
{
    public string? Year { get; set; }
    public List<DataMapperDefinitionRule>? Rules { get; set; }
}

public class DataMapperDefinitionRule
{
    public string Stage { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string ResultQualifier { get; set; } = string.Empty;
    public string ResultField { get; set; } = string.Empty;
}
