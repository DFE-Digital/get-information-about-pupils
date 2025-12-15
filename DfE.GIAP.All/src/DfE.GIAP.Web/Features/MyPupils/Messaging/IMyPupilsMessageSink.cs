namespace DfE.GIAP.Web.Features.MyPupils.Messaging;

public interface IMyPupilsMessageSink
{
    IReadOnlyList<MyPupilsMessage> GetMessages();
    void Add(MyPupilsMessage log);
}
