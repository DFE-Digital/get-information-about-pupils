namespace DfE.GIAP.Web.Features.MyPupils.Logging;

public interface IMyPupilsLogSink
{
    IReadOnlyList<MyPupilsLog> GetLogs();
    void Add(MyPupilsLog log);
}
