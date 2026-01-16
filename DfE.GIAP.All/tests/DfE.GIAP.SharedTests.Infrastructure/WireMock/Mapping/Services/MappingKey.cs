namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
public record MappingKey
{
    public MappingKey(string identifier)
    {
        Guard.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));
        Value = identifier;
    }

    public string Value { get; }
}
