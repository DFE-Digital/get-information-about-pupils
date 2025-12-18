using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.MyPupils.Messaging;
#nullable enable
// Note: temporary log sink to enable commands from Web (Update, Delete) actions to persist messages that survive a redirect that need to be consumed in GET paths for ViewModel properties as part of the PRG pattern

// TODO abstract IJsonSerialiser
public sealed class MyPupilsMessageSink : IMyPupilsMessageSink
{
    private const int MaxLogMessages = 25;
    private readonly IMapper<MyPupilsMessage, MyPupilsMessageDto> _mapToDto;
    private readonly IMapper<MyPupilsMessageDto, MyPupilsMessage> _mapFromDto;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataFactory;
    private readonly MyPupilsMessagingOptions _options;

    public MyPupilsMessageSink(
        IMapper<MyPupilsMessage, MyPupilsMessageDto> mapToDto,
        IMapper<MyPupilsMessageDto, MyPupilsMessage> mapFromDto,
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataFactory,
        IOptionsSnapshot<MyPupilsMessagingOptions> myPupilsLoggingOptions)
    {
        ArgumentNullException.ThrowIfNull(mapToDto);
        _mapToDto = mapToDto;

        ArgumentNullException.ThrowIfNull(mapFromDto);
        _mapFromDto = mapFromDto;

        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;

        ArgumentNullException.ThrowIfNull(tempDataFactory);
        _tempDataFactory = tempDataFactory;

        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions);
        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions.Value);
        _options = myPupilsLoggingOptions.Value;
    }

    public IReadOnlyList<MyPupilsMessage> GetMessages()
    {
        ITempDataDictionary tempData = GetTempData();

        object? raw = tempData[_options.MessagesKey]; // read & remove

        if (raw is not string json || string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<MyPupilsMessage>();
        }

        List<MyPupilsMessageDto>? logs = JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(json);

        return (logs?.Select(_mapFromDto.Map) ?? [])
            .ToList()
            .AsReadOnly();
    }

    public void Add(MyPupilsMessage message)
    {
        (ITempDataDictionary tempData, List<MyPupilsMessage> existingLogs) = Peek();

        existingLogs.Add(message);

        if (existingLogs.Count > MaxLogMessages)
        {
            existingLogs = existingLogs.Skip(existingLogs.Count - MaxLogMessages).ToList();
        }

        tempData[_options.MessagesKey] =
            JsonConvert.SerializeObject(
                value: existingLogs.Select(_mapToDto.Map).ToList());
    }

    private (ITempDataDictionary TempData, List<MyPupilsMessage> Messages) Peek()
    {
        ITempDataDictionary tempData = GetTempData();

        object? peeked = tempData.Peek(_options.MessagesKey);

        if (peeked is null)
        {
            return (tempData, []);
        }

        // TODO should we throw if another area wrote another log, or another serialised type to the shared TempData?
        if (peeked is string json && !string.IsNullOrWhiteSpace(json))
        {
            List<MyPupilsMessageDto>? logs = JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(json);
            return (tempData, (logs?.Select(_mapFromDto.Map) ?? []).ToList());
        }

        return (tempData, []);
    }

    private ITempDataDictionary GetTempData()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        return _tempDataFactory.GetTempData(context);
    }
}
#nullable restore
