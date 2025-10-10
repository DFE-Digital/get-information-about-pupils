namespace DfE.GIAP.Core.MyPupils.Application.Services.Search.Options;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public sealed class SearchIndexOptions
{
    public string Url { get; set; }
    public string Key { get; set; }
    public Dictionary<string, IndexOptions> Indexes { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
