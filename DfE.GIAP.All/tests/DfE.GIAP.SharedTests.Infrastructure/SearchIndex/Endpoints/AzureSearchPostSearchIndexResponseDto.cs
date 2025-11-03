namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints;
#nullable disable   
public record AzureSearchPostSearchIndexResponseDto
{
    public IEnumerable<object> value { get; set; }
}
