namespace DfE.GIAP.SharedTests.Infrastructure.WireMock.Mapping.Services;
public record MappingRequest
{
    public MappingRequest(MappingKey id, MappingModel model)
    {
        Guard.ThrowIfNull(id, nameof(id));
        ClientId = id;

        Guard.ThrowIfNull(model, nameof(model));
        Mapping = model;
    }
    public MappingKey ClientId { get; }
    public MappingModel Mapping { get; }
}
