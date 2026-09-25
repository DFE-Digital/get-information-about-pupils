using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.TempData;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Messaging;
#nullable enable
// Note: temporary log sink to enable commands (Update, Delete) actions to persist messages that survive a redirect that need to be consumed in GET paths for ViewModel properties as part of the PRG pattern. e.g. IsDeleteSuccessful. 

// TODO can we constrain to ensure that ONLY a specific type can be written, than loose type access around TempDataDictionary

public sealed class MyPupilsTempDataMessageSink : IMyPupilsMessageSink
{
    private const int MaxLogMessages = 25;
    private readonly IMapper<MyPupilsMessage, MyPupilsMessageDto> _mapToDto;
    private readonly IMapper<MyPupilsMessageDto, MyPupilsMessage> _mapFromDto;
    private readonly ITempDataDictionaryProvider _tempDataDictionaryProvider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly MyPupilsMessagingOptions _options;

    public MyPupilsTempDataMessageSink(
        IMapper<MyPupilsMessage, MyPupilsMessageDto> mapToDto,
        IMapper<MyPupilsMessageDto, MyPupilsMessage> mapFromDto,
        IOptions<MyPupilsMessagingOptions> myPupilsLoggingOptions,
        ITempDataDictionaryProvider tempDataDictionaryProvider,
        IJsonSerializer jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(mapToDto);
        _mapToDto = mapToDto;

        ArgumentNullException.ThrowIfNull(mapFromDto);
        _mapFromDto = mapFromDto;

        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions);
        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions.Value);
        _options = myPupilsLoggingOptions.Value;

        ArgumentNullException.ThrowIfNull(tempDataDictionaryProvider);
        _tempDataDictionaryProvider = tempDataDictionaryProvider;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;
    }

    public IReadOnlyList<MyPupilsMessage> GetMessages()
    {
        ITempDataDictionary tempData = _tempDataDictionaryProvider.GetTempData();

        object? value = tempData[_options.MessagesKey]; // read & remove

        if (value is null || value is not string json || string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<MyPupilsMessage>();
        }

        _jsonSerializer.TryDeserialize(json, out List<MyPupilsMessageDto>? logs);

        return (logs?.Select(_mapFromDto.Map) ?? [])
            .ToList()
            .AsReadOnly();
    }

    public void AddMessage(MyPupilsMessage message)
    {
        (ITempDataDictionary tempData, List<MyPupilsMessageDto> current) = PeekMessages();

        current.Add(_mapToDto.Map(message));

        string serialisedMessages =
            _jsonSerializer.Serialize(
                current.TakeLast(MaxLogMessages).ToList());

        tempData[_options.MessagesKey] = serialisedMessages;
    }

    private (ITempDataDictionary TempData, List<MyPupilsMessageDto> Messages) PeekMessages()
    {
        ITempDataDictionary tempData = _tempDataDictionaryProvider.GetTempData();

        object? peeked = tempData.Peek(_options.MessagesKey);

        if (peeked is null)
        {
            return (tempData, []);
        }

        // TODO should we throw if another area wrote another log, or another serialised type to the shared TempData?
        if (peeked is string json && !string.IsNullOrWhiteSpace(json))
        {
            _jsonSerializer.TryDeserialize(json, out List<MyPupilsMessageDto>? messages);
            return (tempData, (messages ?? []).ToList());
        }

        return (tempData, []);
    }
}
#nullable restore
