namespace DfE.GIAP.Core.Gias;

public sealed class GiasExtractException(GiasExtractType extractType, string message)
    : Exception($"{extractType} extract: {message}")
{
    public GiasExtractType ExtractType { get; } = extractType;
}
