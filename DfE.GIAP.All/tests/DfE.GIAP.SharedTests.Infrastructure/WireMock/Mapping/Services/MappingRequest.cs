namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
public record MappingRequest
{
    public MappingRequest(ClientKey id, MappingModel model)
    {
        Guard.ThrowIfNull(id, nameof(id));
        ClientId = id;

        Guard.ThrowIfNull(model, nameof(model));
        Mapping = model;
    }
    public ClientKey ClientId { get; }
    public MappingModel Mapping { get; }
}
