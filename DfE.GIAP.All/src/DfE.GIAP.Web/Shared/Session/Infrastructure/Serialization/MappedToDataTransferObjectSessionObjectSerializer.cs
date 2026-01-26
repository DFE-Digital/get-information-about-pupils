using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session.Abstraction;

namespace DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;

public class MappedToDataTransferObjectSessionObjectSerializer<TSessionObject, TDataTransferObject> : ISessionObjectSerializer<TSessionObject>
    where TSessionObject : class
    where TDataTransferObject : class
{
    private readonly IMapper<TSessionObject, TDataTransferObject> _mapToDto;
    private readonly IMapper<TDataTransferObject, TSessionObject> _mapFromDto;
    private readonly IJsonSerializer _jsonSerializer;

    public MappedToDataTransferObjectSessionObjectSerializer(
        IMapper<TSessionObject, TDataTransferObject> toDtoMapper,
        IMapper<TDataTransferObject, TSessionObject> fromDtoMapper,
        IJsonSerializer jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(toDtoMapper);
        _mapToDto = toDtoMapper;

        ArgumentNullException.ThrowIfNull(fromDtoMapper);
        _mapFromDto = fromDtoMapper;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;
    }

    public string Serialize(TSessionObject sessionObject)
    {
        TDataTransferObject dataTransferObject = _mapToDto.Map(sessionObject);
        return _jsonSerializer.Serialize(dataTransferObject);
    }

    public TSessionObject Deserialize(string input)
    {
        TDataTransferObject dataTransferObject = _jsonSerializer.Deserialize<TDataTransferObject>(input);
        TSessionObject sessionObject = _mapFromDto.Map(dataTransferObject);
        return sessionObject;
    }
}
