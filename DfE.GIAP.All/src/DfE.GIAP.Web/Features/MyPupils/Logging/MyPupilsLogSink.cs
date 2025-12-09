using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Logging;
#nullable enable
public sealed class MyPupilsLogSink : IMyPupilsLogSink
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataFactory;
    private readonly IOptionsSnapshot<MyPupilsLoggingOptions> _myPupilsLoggingOptions;

    public MyPupilsLogSink(
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataFactory,
        IOptionsSnapshot<MyPupilsLoggingOptions> myPupilsLoggingOptions)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;

        ArgumentNullException.ThrowIfNull(tempDataFactory);
        _tempDataFactory = tempDataFactory;

        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions);
        ArgumentNullException.ThrowIfNull(myPupilsLoggingOptions.Value);
        _myPupilsLoggingOptions = myPupilsLoggingOptions;
    }

    // Logs are destroyed after read when using TempData
    public IReadOnlyList<MyPupilsLog> GetLogs()
    {
        ITempDataDictionary tempData = GetTempDataFromHttpContext();
        tempData.TryGetValue(_myPupilsLoggingOptions.Value.MessagesKey, out object? stored);
        return (stored as List<MyPupilsLog>) ?? [];
    }

    public void Add(MyPupilsLog log)
    {
        if (log.Level < ParseMinimumLoggingLevel(_myPupilsLoggingOptions.Value.MinimumLogLevel))
        {
            return;
        }

        ITempDataDictionary tempData = GetTempDataFromHttpContext();

        if (tempData.Peek(_myPupilsLoggingOptions.Value.MessagesKey) is not List<MyPupilsLog> stored) // TODO we should throw here as 2 things are using TempDataDictionary
        {
            stored = [];
            tempData[_myPupilsLoggingOptions.Value.MessagesKey] = stored;
        }

        stored.Add(log);
    }

    private ITempDataDictionary GetTempDataFromHttpContext()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(context);

        ITempDataDictionary tempData = _tempDataFactory.GetTempData(context);

        return tempData;
    }

    private static LogLevel ParseMinimumLoggingLevel(string input, LogLevel defaultLevel = LogLevel.Error)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultLevel;
        }

        // Try direct enum names: "Debug", "Info", "Error" (case-insensitive)
        if (Enum.TryParse(input, ignoreCase: true, out LogLevel byName))
        {
            return byName;
        }

        return defaultLevel;
    }
}
