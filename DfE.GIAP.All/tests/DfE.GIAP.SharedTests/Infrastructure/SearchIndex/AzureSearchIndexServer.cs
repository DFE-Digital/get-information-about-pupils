using System.Text;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

internal sealed class AzureSearchIndexHttpClient : IDisposable
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:8443")
    };

    public async Task StubIndexListResponse(params string[] indexNames)
    {
        var responseBody = new
        {
            value = indexNames.Select((name) => new
            {
                name
            })
        };

        var stub = new
        {
            request = new
            {
                method = "GET",
                url = "/indexes?$select=name&api-version=2025-09-01"
            },
            response = new
            {
                status = 200,
                headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                },
                jsonBody = responseBody
            }
        };

        StringContent content = new(
            content: JsonConvert.SerializeObject(stub),
            encoding: Encoding.UTF8,
            mediaType: "application/json");

        await _httpClient.PostAsync("/__admin/mappings", content);
    }

    public async Task StubSearchResponseForIndex(
        string indexName,
        IEnumerable<AzureIndexEntity> indexDocuments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        var indexResponse = new
        {
            value = indexDocuments?.Select(t => new
            {
                @searchScore = t.Score,
                t.id,
                t.UPN,
                t.Surname,
                t.Forename,
                t.Sex,
                t.DOB,
                t.LocalAuthority
            }) ?? [],
        };

        var stub = new
        {
            request = new
            {
                method = "POST",
                url = $"/indexes('{indexName}')/docs/search.post.search?api-version=2025-09-01",
            },
            response = new
            {
                status = 200,
                headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
                jsonBody = indexResponse
            }
        };

        StringContent content = new(JsonConvert.SerializeObject(stub), Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("/__admin/mappings", content);
    }

    public void Dispose()
    {
        // TODO dispose?
        //_httpClient.Dispose();
    }
}
