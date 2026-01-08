namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

/// <summary>
/// Represents an event that records details about a file download action performed by a user within an organization
/// session.
/// </summary>
/// <param name="UserId">The unique identifier of the user who performed the download.</param>
/// <param name="SessionId">The identifier of the session in which the download occurred.</param>
/// <param name="Description">A description providing additional context about the download event.</param>
/// <param name="OrgURN">The Uniform Resource Name (URN) of the organization associated with the event.</param>
/// <param name="OrgName">The name of the organization where the download took place.</param>
/// <param name="OrgCategory">The category or type of the organization related to the event.</param>
/// <param name="Payload">The payload containing specific details about the downloaded file or resource.</param>
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

/// <summary>
/// Represents the payload for a download event, including details about the download type, file format, event type,
/// batch identifier, and associated dataset.
/// </summary>
/// <param name="DownloadType">The type of download being performed. Determines the nature of the download operation.</param>
/// <param name="DownloadFileFormat">The format of the file to be downloaded. Specifies how the downloaded data will be structured.</param>
/// <param name="DownloadEventType">The event type associated with the download, if applicable. May be null if the event type is not specified.</param>
/// <param name="BatchId">The identifier for the batch associated with the download, if any. May be null if the download is not part of a
/// batch.</param>
/// <param name="Dataset">The dataset related to the download operation, if available. May be null if no dataset is associated.</param>
public record DownloadPayload(
    DownloadType DownloadType,
    DownloadFileFormat DownloadFileFormat,
    DownloadEventType? DownloadEventType,
    string? BatchId,
    Dataset? Dataset) : IEventPayload;

