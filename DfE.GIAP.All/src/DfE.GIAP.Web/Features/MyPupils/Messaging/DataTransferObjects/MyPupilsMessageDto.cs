namespace DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;

public class MyPupilsMessageDto
{
    public MessageLevel MessageLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}
