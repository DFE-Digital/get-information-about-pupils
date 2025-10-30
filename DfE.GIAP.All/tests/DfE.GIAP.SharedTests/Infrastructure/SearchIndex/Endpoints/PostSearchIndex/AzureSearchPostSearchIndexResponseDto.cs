namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints.PostSearchIndex;
#nullable disable   
public record AzureSearchPostSearchIndexResponseDto
{
    public IEnumerable<AzureSearchIndexSearchResponseDto> value { get; set; }
}
public record AzureSearchIndexSearchResponseDto
{

    public string @searchScore { get; set; }
    public string id { get; set; }
    public string UPN { get; set; }
    public string Surname { get; set; }
    public string Forename { get; set; }
    public string Sex { get; set; }
    public string DOB { get; set; }
    public string LocalAuthority { get; set; }
}
