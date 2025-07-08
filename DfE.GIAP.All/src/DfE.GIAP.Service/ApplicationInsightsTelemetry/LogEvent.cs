namespace DfE.GIAP.Service.ApplicationInsightsTelemetry;

public class LogEvent
{
    public string FrameworkFunction { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; }
    public string CheckPoint { get; set; }
    public string Status { get; set; }
    public string EntityType { get; set; }
}