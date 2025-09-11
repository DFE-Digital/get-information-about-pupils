using Newtonsoft.Json;

namespace DfE.GIAP.Core.Common.Infrastructure.Logging;


public interface ILogFormatter
{
    string Format(LogEntry entry);
    Dictionary<string, string>? FormatStructured(LogEntry entry);
}

public class PlainTextFormatter : ILogFormatter
{
    public string Format(LogEntry entry)
    {
        return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Level}: {entry.Message}";
    }

    public Dictionary<string, string>? FormatStructured(LogEntry entry) => null;
}

public class JsonFormatter : ILogFormatter
{
    public string Format(LogEntry entry)
    {
        return JsonConvert.SerializeObject(entry);
    }

    public Dictionary<string, string>? FormatStructured(LogEntry entry)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>
        {
            ["Level"] = entry.Level.ToString(),
            ["Message"] = entry.Message,
            ["Timestamp"] = entry.Timestamp?.ToString("o") ?? DateTime.UtcNow.ToString("o")
        };

        if (entry.Context is not null)
            dict["Context"] = JsonConvert.SerializeObject(entry.Context);

        if (entry.Exception is not null)
            dict["Exception"] = entry.Exception.ToString();

        return dict;
    }
}
