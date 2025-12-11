namespace DfE.GIAP.Web.Features.MyPupils.Messaging;

public record MyPupilsMessagingOptions
{
    public string DeleteSuccessfulKey { get; set; } = "DeleteSuccessful";
    public string LogsKey { get; internal set; } = "MyPupilsLogs";
}
