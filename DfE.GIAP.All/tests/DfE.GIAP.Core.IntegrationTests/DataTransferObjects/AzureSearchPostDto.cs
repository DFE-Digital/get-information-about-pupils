using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;

namespace DfE.GIAP.Core.IntegrationTests.DataTransferObjects;
public record AzureSearchPostDto
{
    public IEnumerable<AzureNpdSearchResponseDto>? value { get; set; }
}
