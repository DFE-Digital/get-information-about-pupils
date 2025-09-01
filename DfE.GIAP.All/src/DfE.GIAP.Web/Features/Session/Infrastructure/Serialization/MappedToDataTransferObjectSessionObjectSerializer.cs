using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.Session.Abstractions;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

public class MappedToDataTransferObjectSessionObjectSerializer<TSessionObject, TDataTransferObject> : ISessionObjectSerializer<TSessionObject>
{
    private readonly IMapper<TSessionObject, TDataTransferObject> _mapToDto;
    private readonly IMapper<TDataTransferObject, TSessionObject> _mapFromDto;

    public MappedToDataTransferObjectSessionObjectSerializer(
        IMapper<TSessionObject, TDataTransferObject> toDtoMapper,
        IMapper<TDataTransferObject, TSessionObject> fromDtoMapper)
    {
        ArgumentNullException.ThrowIfNull(toDtoMapper);
        _mapToDto = toDtoMapper;

        ArgumentNullException.ThrowIfNull(fromDtoMapper);
        _mapFromDto = fromDtoMapper;
    }

    public string Serialize(TSessionObject sessionObject)
    {
        TDataTransferObject dataTransferObject = _mapToDto.Map(sessionObject);
        return JsonConvert.SerializeObject(dataTransferObject);
    }

    public TSessionObject Deserialize(string input)
    {
        TDataTransferObject dataTransferObject = JsonConvert.DeserializeObject<TDataTransferObject>(input);
        TSessionObject sessionObject = _mapFromDto.Map(dataTransferObject);
        return sessionObject;
    }
}
