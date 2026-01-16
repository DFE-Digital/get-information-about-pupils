namespace DfE.GIAP.Web.Features.MyPupils.Messaging;

#nullable enable
public record MyPupilsMessage
{
    public MyPupilsMessage(string? id, MessageLevel level, string message)
    {
        Id = string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();

        ArgumentNullException.ThrowIfNull(level);
        Level = level;

        Message = message?.Trim() ?? string.Empty;
    }

    public MyPupilsMessage(MessageLevel level, string message) : this(null, level, message)
    {
    }

    public string Id { get; }
    public MessageLevel Level { get; }
    public string Message { get; }

    public static MyPupilsMessage Create(string id, MessageLevel level, string message)
        => new(id, level, message);

    public static MyPupilsMessage Create(MessageLevel level, string message)
        => new(level, message);
}
