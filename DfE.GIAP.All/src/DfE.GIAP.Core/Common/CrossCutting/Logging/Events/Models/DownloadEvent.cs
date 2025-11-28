namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public record DownloadEvent(
    string UserId,
    string SessionId,
    string Description,
    string OrgURN,
    string OrgName,
    string OrgCategory,
    DownloadPayload Payload)
    : BusinessEvent<DownloadPayload>(UserId, SessionId, Description, OrgURN, OrgName, OrgCategory, Payload)
{
    public override string EventName => nameof(DownloadEvent);
}


public record DownloadPayload(
    DownloadType DownloadType,
    DownloadFileFormat DownloadFileFormat,
    DownloadEventType? DownloadEventType,
    string? BatchId,
    Dataset? Dataset) : IEventPayload;

