namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
public record ClientMappingId
{
    public ClientMappingId(string identifier)
    {
        Guard.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));
        Value = identifier;
    }

    public string Value { get; }
}
