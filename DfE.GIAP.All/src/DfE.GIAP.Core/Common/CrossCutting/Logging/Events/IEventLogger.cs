namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

/// <summary>
/// Defines methods for logging user events such as searches, downloads, and sign-ins within an application.
/// </summary>
/// <remarks>Implementations of this interface should record event details for auditing, analytics, or monitoring
/// purposes. The specific logging mechanism and storage are determined by the implementing class. All methods are
/// intended to be called when the corresponding user action occurs.</remarks>
public interface IEventLogger
{
    /// <summary>
    /// Logs details about a search operation, including its identifier type, whether it is a custom search, and the
    /// applied filter flags.
    /// </summary>
    /// <param name="searchIdentifierType">The type of identifier used to distinguish the search operation. Determines how the search is categorized in the
    /// logs.</param>
    /// <param name="isCustomSearch">Indicates whether the search is a custom search. Set to <see langword="true"/> for custom searches; otherwise,
    /// <see langword="false"/>.</param>
    /// <param name="filterFlags">A dictionary containing filter names and their enabled status. Each key represents a filter, and the
    /// corresponding value indicates whether the filter was applied (<see langword="true"/>) or not (<see
    /// langword="false"/>). Cannot be null.</param>
    void LogSearch(SearchIdentifierType searchIdentifierType, bool isCustomSearch, Dictionary<string, bool> filterFlags);

    /// <summary>
    /// Logs a download event with the specified type, format, and optional contextual information.
    /// </summary>
    /// <param name="downloadType">The type of download being logged. Specifies the category or purpose of the download event.</param>
    /// <param name="downloadFormat">The format of the downloaded file. Determines the file type associated with the download.</param>
    /// <param name="downloadEventType">An optional event type that further classifies the download event. If null, no specific event type is recorded.</param>
    /// <param name="batchId">An optional batch identifier associated with the download. If provided, links the event to a specific batch
    /// operation.</param>
    /// <param name="dataset">An optional dataset related to the download event. If specified, associates the event with a particular dataset.</param>
    void LogDownload(DownloadOperationType downloadType, DownloadFileFormat downloadFormat,
        DownloadEventType? downloadEventType = null, string? batchId = null, Dataset? dataset = null);

    /// <summary>
    /// Logs a user sign-in event with associated session and organization details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who signed in. Cannot be null or empty.</param>
    /// <param name="sessionId">The identifier for the user's session during sign-in. Cannot be null or empty.</param>
    /// <param name="orgUrn">The Uniform Resource Name (URN) of the organization associated with the sign-in. Cannot be null or empty.</param>
    /// <param name="orgName">The display name of the organization associated with the sign-in. Cannot be null or empty.</param>
    /// <param name="orgCategory">The category or type of the organization (for example, 'Education', 'Enterprise'). Cannot be null or empty.</param>
    void LogSignin(string userId, string sessionId, string orgUrn, string orgName, string orgCategory);
}
