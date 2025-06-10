using DfE.GIAP.Core.Content;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.IntegrationTests.Content;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetContentByPageKeyUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _dbFixture;

    public GetContentByPageKeyUseCaseIntegrationTests(CosmosDbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    public async Task InitializeAsync() => await _dbFixture.Database.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetContentByPageKeyUseCase_RetrievesPageContentKey_When_PageKeyIsFound()
    {
        // Arrange
        List<ContentDTO> content = ContentDTOTestDoubles.Generate(count: 10);
        content[0].id = "DocumentId1";
        content[1].id = "DocumentId2";
        await Parallel.ForEachAsync(content, async (t, ctx) => await _dbFixture.Database.WriteAsync(t));

        Dictionary<string, string> contentRepositoryOptions = new()
        {

            ["PageContentOptions:Content:TestPage1:0:Key"] = "TestContentKey1",
            ["PageContentOptions:Content:TestPage1:1:Key"] = "TestContentKey2",
            // RepositoryOptions
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey1:DocumentId"] = "DocumentId1",
            ["ContentRepositoryOptions:ContentKeyToDocumentMapping:TestContentKey2:DocumentId"] = "DocumentId2",
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
            .WithConfiguration(contentRepositoryOptions)
            .WithLocalCosmosDb()
            .Build();

        IServiceCollection services = ServiceCollectionTestDoubles.Default()
            .AddTestServices()
            .RemoveAll<IConfiguration>() // replace default configuration
            .AddSingleton<IConfiguration>(configuration)
            .AddContentDependencies(configuration);

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>>()!;

        // Act
        GetContentByPageKeyUseCaseRequest request = new(pageKey: "TestPage1");
        GetContentByPageKeyUseCaseResponse response = await sut.HandleRequest(request);

        // Assert
        Assert.NotNull(response);
        List<ContentResultItem> results = response.ContentResultItems.ToList();
        Assert.Equal(2, response.ContentResultItems.Count());
        
        Assert.Equal("TestContentKey1", results[0].Key);
        Assert.NotNull(results[0].Content);
        Assert.Equal(content[0].Title, results[0].Content!.Title);
        Assert.Equal(content[0].Body, results[0].Content!.Body);

        Assert.NotNull(results[1].Content);
        Assert.Equal("TestContentKey2", results[1].Key);
        Assert.Equal(content[1].Title, results[1].Content!.Title);
        Assert.Equal(content[1].Body, results[1].Content!.Body);
    }
}
