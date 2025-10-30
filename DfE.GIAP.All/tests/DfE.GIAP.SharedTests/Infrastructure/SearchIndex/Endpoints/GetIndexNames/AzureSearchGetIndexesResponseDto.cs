namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints.GetIndexNames;
# nullable disable
public record AzureSearchGetIndexesResponseDto
{
    public IEnumerable<AzureSearchIndexResponseDto> value { get; set; }
}

public record AzureSearchIndexResponseDto
{
    public string name { get; set; }
}
