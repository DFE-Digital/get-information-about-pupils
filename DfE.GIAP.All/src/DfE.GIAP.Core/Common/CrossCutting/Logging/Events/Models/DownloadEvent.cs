namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

public class DownloadEvent : BusinessEvent
{
    public DownloadType DownloadType { get; }
    public DownloadFileFormat DownloadFileFormat { get; }
    public DownloadEventType? DownloadEventType { get; }
    public string? BatchId { get; }
    public Dataset? Dataset { get; }

    public DownloadEvent(string userId, string sessionId, string description, string orgURN, string orgName, string orgCategory,
                         DownloadType downloadType, DownloadFileFormat downloadFormat, DownloadEventType? downloadEventType = null,
                         string? batchId = null, Dataset? dataset = null)
        : base(userId, sessionId, description, orgURN, orgName, orgCategory)
    {
        DownloadType = downloadType;
        DownloadFileFormat = downloadFormat;
        DownloadEventType = downloadEventType;
        BatchId = batchId;
        Dataset = dataset;
    }

    public override string EventName => "DownloadEvent";
    public override IDictionary<string, string> ToProperties()
    {
        IDictionary<string, string> props = base.ToProperties();
        props["DownloadType"] = DownloadType.ToString();
        props["DownloadFileFormat"] = DownloadFileFormat.ToString();
        props["DownloadEventType"] = DownloadEventType.ToString() ?? string.Empty;
        props["BatchId"] = BatchId ?? string.Empty;
        props["Dataset"] = Dataset.ToString() ?? string.Empty;
        return props;
    }
}

