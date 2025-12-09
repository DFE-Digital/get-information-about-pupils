namespace DfE.GIAP.Web.Features.MyPupils.Logging;

public record MyPupilsLog
{
    public MyPupilsLog(LogLevel level, string message)
    {
        ArgumentNullException.ThrowIfNull(level);
        Level = level;

        ArgumentException.ThrowIfNullOrEmpty(message);
        Message = message;
    }

    public LogLevel Level { get; }
    public string Message { get; }
}
