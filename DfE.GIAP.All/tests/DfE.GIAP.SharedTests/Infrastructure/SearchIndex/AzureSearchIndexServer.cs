using System.Text;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.Infrastructure.SearchIndex;

internal sealed class AzureSearchIndexServer : IDisposable
{
    private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:8443") };

    public AzureSearchIndexServer(IOptions<SearchIndexOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        if (!Uri.TryCreate(options.Value.Url, uriKind: UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException($"Unable to create Search Mock fixture with Url {options.Value.Url}");
        }
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
        // TODO Dispose
    }
}
