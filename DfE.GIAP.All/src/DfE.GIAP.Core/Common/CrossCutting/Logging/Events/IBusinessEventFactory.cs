using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

/// <summary>
/// Defines methods for creating business event objects representing user actions such as downloads, searches, and
/// sign-ins.
/// </summary>
/// <remarks>Implementations of this interface provide a standardized way to construct event objects for tracking
/// and auditing purposes. The created events can be used for logging, analytics, or compliance workflows.</remarks>
public interface IBusinessEventFactory
{
    DownloadEvent CreateDownload(DownloadType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null);
    SearchEvent CreateSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags);
    SigninEvent CreateSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}
