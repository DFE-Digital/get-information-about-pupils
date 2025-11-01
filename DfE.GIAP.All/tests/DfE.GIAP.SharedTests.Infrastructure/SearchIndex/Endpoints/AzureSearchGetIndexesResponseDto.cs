namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints;
# nullable disable
internal record AzureSearchGetIndexesResponseDto
{
    public IEnumerable<SearchIndexResponseDto> value { get; set; }
}

internal record SearchIndexResponseDto
{
    public string name { get; set; }
}
