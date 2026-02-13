namespace DfE.GIAP.Core.Downloads.Application.Ctf.Options;

public class CtfSchemaCacheOptions
{
    public const string SectionName = "CtfSchemaCacheOptions";

    public int CacheDays { get; set; } = 1;
    public int CacheHours { get; set; } = 0;
    public int CacheMinutes { get; set; } = 0;

    public TimeSpan ToTimeSpan() =>
        new(CacheDays, CacheHours, CacheMinutes, 0);
}
