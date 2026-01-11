namespace DfE.GIAP.Web.Features.MyPupils.Messaging;

public record MyPupilsMessagingOptions
{
    public string DeleteSuccessfulKey { get; set; } = "DeleteSuccessful";
    public string MessagesKey { get; internal set; } = "MyPupilsLogs";
}