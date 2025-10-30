using System.Net;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints.GetIndexNames;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex.Endpoints.PostSearchIndex;
using DfE.GIAP.SharedTests.Infrastructure.WireMock;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

internal sealed class AzureSearchIndex
{
    private readonly IWireMockClient _wireMockClient;

    public AzureSearchIndex(IWireMockClient wireMockClient)
    {
        ArgumentNullException.ThrowIfNull(wireMockClient);
        _wireMockClient = wireMockClient;
    }

    public async Task StubIndexListResponse(params string[] indexNames)
    {
        AzureSearchGetIndexesResponseDto dto = new()
        {
            value = indexNames.Select((index) => new AzureSearchIndexResponseDto
            {
                name = index
            })
        };

        RequestMatch request = new(
            path: "/indexes",
            method: HttpMethod.Get,
            queryParams: [
                new("api-version", "2025-09-01"),
                new("$select", "name")
            ]);

        Response<AzureSearchGetIndexesResponseDto> response = new(HttpStatusCode.OK, dto);

        await _wireMockClient.Stub(request, response);
    }

    public async Task StubIndexSearchResponse(
        string indexName,
        IEnumerable<AzureIndexEntity> indexDocuments)
    {
        AzureSearchPostSearchIndexResponseDto dto = new()
        {
            value = indexDocuments.Select((document) =>
            {
                return new AzureSearchIndexSearchResponseDto()
                {
                    @searchScore = document.Score,
                    id = document.id,
                    UPN = document.UPN,
                    Forename = document.Forename,
                    Surname = document.Surname,
                    DOB = document.DOB?.ToString("yyyy-MM-dd") ?? null,
                    Sex = document.Sex,
                    LocalAuthority = document.LocalAuthority
                };
            })
        };

        RequestMatch request = new(
            path: $"/indexes('{indexName}')/docs/search.post.search",
            method: HttpMethod.Post,
            queryParams: [
                new("api-version", "2025-09-01")
            ]);

        Response<AzureSearchPostSearchIndexResponseDto> response = new(HttpStatusCode.OK, dto);

        await _wireMockClient.Stub(request, response);
    }
}
